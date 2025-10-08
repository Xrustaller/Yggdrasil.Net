using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.Minecraft.AspShared.Validation;

public class AllRequiredValidatorsRegisteredService
{
    private readonly IServiceProvider _services;

    public AllRequiredValidatorsRegisteredService(IServiceProvider services)
    {
        _services = services;
    }

    public IReadOnlyList<ControllerActionValidationResult> CheckControllersParams(params Assembly[] skipAssemblies)
    {
        ActionDescriptorCollection actionDescriptors = _services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors;
        ControllerActionDescriptor[] controllerDescriptors = actionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .ToArray();

        List<ControllerActionValidationResult> results = new();
        foreach (ControllerActionDescriptor descriptor in controllerDescriptors)
        {
            List<ControllerActionValidationResult.ArgumentValidationInfo> paramResults = new();

            ControllerParameterDescriptor[] actionParams = descriptor.Parameters
                .Where(x => x.BindingInfo?.BindingSource?.IsFromRequest == true)
                .Cast<ControllerParameterDescriptor>()
                .ToArray();
            foreach (ControllerParameterDescriptor param in actionParams)
            {
                ControllerActionValidationResult.ArgumentValidationInfo info = new() { ParameterInfo = param.ParameterInfo };
                paramResults.Add(info);
                SkipValidatorsCheckAttribute? skipAttr = param.ParameterInfo.GetCustomAttribute<SkipValidatorsCheckAttribute>();
                if (skipAttr != null)
                {
                    info.SkipType = ControllerActionValidationResult.ValidationSkipType.Attribute;
                    info.SkipReason = skipAttr.Reason;
                    continue;
                }

                Type type = Nullable.GetUnderlyingType(param.ParameterType) ?? param.ParameterType;
                if (skipAssemblies.Any(x => x == type.Assembly))
                {
                    info.SkipType = ControllerActionValidationResult.ValidationSkipType.Assembly;
                    info.SkipReason = "Assembly skip";
                    continue;
                }

                bool validatorRequired = (type.IsClass || type.IsValueType) &&
                                         !type.IsPrimitive &&
                                         !type.Namespace!.StartsWith("System.");
                if (!validatorRequired)
                {
                    info.SkipType = ControllerActionValidationResult.ValidationSkipType.MemberType;
                    info.SkipReason = $"Type {type.Name} always skip";
                    continue;
                }

                Type validatorType = typeof(IValidator<>).MakeGenericType(type);
                object? validator = _services.GetService(validatorType);
                info.ValidatorFound = validator != null;
            }

            results.Add(new ControllerActionValidationResult
            {
                Arguments = paramResults,
                ControllerName = descriptor.ControllerName,
                ActionName = descriptor.ActionName
            });
        }

        if (results
            .SelectMany(x => x.Arguments)
            .Any(x => x is { SkipType: ControllerActionValidationResult.ValidationSkipType.No, ValidatorFound: false }))
            throw new HaveMissedValidatorsException(results);

        return results;
    }
}
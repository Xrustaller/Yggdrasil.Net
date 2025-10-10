using System.Security.Cryptography;
using System.Text;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.YggdrasilApi.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArkProjects.Minecraft.YggdrasilApi.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ServiceAuthAttribute : Attribute, IAsyncActionFilter
{
    public bool RequireCreateOtherService { get; set; } = false;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        IServiceServerService serviceServer = ctx.HttpContext.RequestServices.GetRequiredService<IServiceServerService>();
        ILogger<ServiceAuthAttribute> logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<ServiceAuthAttribute>>();

        string? serviceName = ctx.HttpContext.Request.Headers["X-Service-Name"].FirstOrDefault();
        string? timestamp = ctx.HttpContext.Request.Headers["X-Timestamp"].FirstOrDefault();
        string? signature = ctx.HttpContext.Request.Headers["X-Signature"].FirstOrDefault();

        if (serviceName == null || timestamp == null || signature == null)
        {
            ctx.Result = new UnauthorizedObjectResult("Missing service auth headers");
            return;
        }

        if (!long.TryParse(timestamp, out long tsVal))
        {
            ctx.Result = new UnauthorizedObjectResult("Invalid timestamp");
            return;
        }

        DateTimeOffset ts = DateTimeOffset.FromUnixTimeSeconds(tsVal);
        if (Math.Abs((DateTimeOffset.UtcNow - ts).TotalMinutes) > 5)
        {
            ctx.Result = new UnauthorizedObjectResult("Timestamp expired");
            return;
        }

        ServiceEntity? service = await serviceServer.GetServiceAsync(serviceName);
        if (service == null)
        {
            ctx.Result = new UnauthorizedObjectResult("Unknown service");
            return;
        }

        string payload = $"{serviceName}:{timestamp}";
        using HMACSHA256 hmac = new(Convert.FromBase64String(service.Secret));
        string expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        if (RequireCreateOtherService && !service.CreateOtherService)
        {
            ctx.Result = new UnauthorizedObjectResult("Access Denied");
            return;
        }

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature)))
        {
            ctx.Result = new UnauthorizedObjectResult("Invalid signature");
            return;
        }

        await next();
    }
}
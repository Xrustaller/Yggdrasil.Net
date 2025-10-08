namespace ArkProjects.Minecraft.AspShared.Validation;

[AttributeUsage(AttributeTargets.Parameter)]
public class SkipValidatorsCheckAttribute : Attribute
{
    public SkipValidatorsCheckAttribute(string reason)
    {
        Reason = reason;
    }

    public string Reason { get; }
}
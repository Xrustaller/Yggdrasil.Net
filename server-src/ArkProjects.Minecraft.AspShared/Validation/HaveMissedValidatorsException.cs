using System.Text;

namespace ArkProjects.Minecraft.AspShared.Validation;

public class HaveMissedValidatorsException : Exception
{
    public HaveMissedValidatorsException(IReadOnlyList<ControllerActionValidationResult> results) : base(
        BuildMessage(results))
    {
        Results = results;
    }

    public IReadOnlyList<ControllerActionValidationResult> Results { get; }

    private static string BuildMessage(IReadOnlyList<ControllerActionValidationResult> results)
    {
        StringBuilder sb = new();
        sb.AppendLine("Detect missing validators in controllers");
        foreach (ControllerActionValidationResult cav in results) sb.AppendLine(cav.ToString());

        return sb.ToString();
    }
}
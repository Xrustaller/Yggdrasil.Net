using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models;

public class ErrorResponse
{
    [JsonPropertyName("error")] public required string Error { get; set; }

    [JsonPropertyName("errorMessage")] public required string ErrorMessage { get; set; }

    [JsonPropertyName("cause")] public string? Cause { get; set; }

    [JsonIgnore] public int StatusCode { get; set; }
}
using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class ValidateRequest
{
    [JsonPropertyName("clientToken")] public required string ClientToken { get; set; }

    [JsonPropertyName("accessToken")] public required string AccessToken { get; set; }
}
using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class AuthenticateRequest
{
    [JsonPropertyName("agent")] public ClientAgentModel? Agent { get; set; }

    [JsonPropertyName("username")] public string LoginOrEmail { get; set; } = null!;

    [JsonPropertyName("password")] public string Password { get; set; } = null!;

    [JsonPropertyName("clientToken")] public string? ClientToken { get; set; }

    [JsonPropertyName("requestUser")] public bool RequestUser { get; set; }
}
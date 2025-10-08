using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class SignOutRequest
{
    [JsonPropertyName("username")] public string UserName { get; set; } = null!;

    [JsonPropertyName("password")] public string Password { get; set; } = null!;
}
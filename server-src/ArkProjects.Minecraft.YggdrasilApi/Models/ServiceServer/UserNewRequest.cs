using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class UserNewRequest
{
    [JsonPropertyName("login")] public string Login { get; set; } = null!;

    [JsonPropertyName("email")] public string Email { get; set; } = null!;

    [JsonPropertyName("password")] public string Password { get; set; } = null!;
}
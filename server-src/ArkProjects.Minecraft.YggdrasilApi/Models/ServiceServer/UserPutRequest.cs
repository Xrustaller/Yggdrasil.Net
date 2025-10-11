using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class UserPutRequest
{
    [JsonPropertyName("login")] public string? Login { get; set; } = null!;

    [JsonPropertyName("email")] public string? Email { get; set; } = null!;

    [JsonPropertyName("password")] public string? Password { get; set; } = null!;
    
    [JsonPropertyName("set_delete")] public bool? SetDelete { get; set; } = false!;
}
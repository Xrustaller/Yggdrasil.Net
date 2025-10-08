using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class RefreshResponse
{
    [JsonPropertyName("user")] public UserModel? User { get; set; }

    [JsonPropertyName("clientToken")] public required string ClientToken { get; set; }

    [JsonPropertyName("accessToken")] public required string AccessToken { get; set; }

    [JsonPropertyName("selectedProfile")] public UserProfileModel? SelectedProfile { get; set; }
}
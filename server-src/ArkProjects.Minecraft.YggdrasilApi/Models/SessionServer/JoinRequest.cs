using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.SessionServer;

public class JoinRequest
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("selectedProfile")] public Guid SelectedProfile { get; set; }

    [JsonPropertyName("serverId")] public string ServerId { get; set; } = null!;
}
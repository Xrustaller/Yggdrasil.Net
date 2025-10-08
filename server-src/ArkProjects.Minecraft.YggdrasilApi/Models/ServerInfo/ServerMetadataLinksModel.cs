using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServerInfo;

public class ServerMetadataLinksModel
{
    [JsonPropertyName("homepage")] public required string HomePage { get; set; }

    [JsonPropertyName("register")] public required string Register { get; set; }
}
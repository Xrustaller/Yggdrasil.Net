using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServerInfo;

public class ServerMetadataModel
{
    [JsonPropertyName("implementationName")]
    public required string ImplementationName { get; set; }

    [JsonPropertyName("implementationVersion")]
    public required string ImplementationVersion { get; set; }

    [JsonPropertyName("serverName")] public required string ServerName { get; set; }

    [JsonPropertyName("feature.non_email_login")]
    public bool FeatureNonEmailLogin { get; set; }

    [JsonPropertyName("links")] public ServerMetadataLinksModel Links { get; set; }
}
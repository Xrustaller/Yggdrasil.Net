using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServerInfo;

public class ServerInfoModel
{
    [JsonPropertyName("meta")] public ServerMetadataModel? Meta { get; set; }

    [JsonPropertyName("skinDomains")] public IReadOnlyList<string> SkinDomains { get; set; }

    [JsonPropertyName("signaturePublickey")]
    public string? SignaturePublicKey { get; set; }
}
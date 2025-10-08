using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.SessionServer;

public class ProfileTextureModel
{
    public const string SkinTextureName = "SKIN";
    public const string CapeTextureName = "CAPE";

    [JsonPropertyName("url")] public required string Url { get; set; }

    [JsonPropertyName("metadata")] public required Dictionary<string, string> Metadata { get; set; }
}
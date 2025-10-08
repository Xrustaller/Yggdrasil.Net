using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class UserPropertyModel
{
    public const string PreferredLangKey = "preferredLanguage";

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("value")] public required string Value { get; set; }
}
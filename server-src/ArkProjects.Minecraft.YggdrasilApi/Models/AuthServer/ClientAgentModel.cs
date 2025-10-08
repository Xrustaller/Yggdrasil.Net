using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class ClientAgentModel
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("version")] public int? Version { get; set; }
}
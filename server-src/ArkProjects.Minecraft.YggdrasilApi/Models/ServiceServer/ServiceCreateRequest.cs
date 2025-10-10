using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class ServiceCreateRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("create_other_service")]
    public bool CreateOtherService { get; set; } = false;
}
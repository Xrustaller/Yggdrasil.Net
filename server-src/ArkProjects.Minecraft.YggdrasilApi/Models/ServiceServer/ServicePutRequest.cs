using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class ServicePutRequest
{
    [JsonPropertyName("create_other_service")]
    public bool? CreateOtherService { get; set; }
}
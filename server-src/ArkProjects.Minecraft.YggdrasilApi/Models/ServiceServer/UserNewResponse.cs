using System.Text.Json.Serialization;
using ArkProjects.Minecraft.Database.Entities.Users;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class UserNewResponse
{
    public UserNewResponse()
    {
    }

    public UserNewResponse(UserEntity user)
    {
        UserId = user.Id;
    }

    [JsonPropertyName("user_id")] public Guid UserId { get; set; } = Guid.Empty;
}
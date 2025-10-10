using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class UsersGetResponse
{
    public UsersGetResponse()
    {
    }

    public UsersGetResponse(List<UserGetResponse> users)
    {
        Users = users;
    }

    [JsonPropertyName("users")] public List<UserGetResponse> Users { get; set; } = null!;
}
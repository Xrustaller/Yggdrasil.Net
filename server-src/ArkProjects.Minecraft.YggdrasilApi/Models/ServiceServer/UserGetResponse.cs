using System.Text.Json.Serialization;
using ArkProjects.Minecraft.Database.Entities.Users;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

public class UserGetResponse
{
    public UserGetResponse()
    {
    }

    public UserGetResponse(UserEntity user)
    {
        UserId = user.Id;
        Login = user.Login;
        Email = user.Email;
    }

    [JsonPropertyName("user_id")] public Guid UserId { get; set; } = Guid.Empty;

    [JsonPropertyName("login")] public string Login { get; set; } = null!;

    [JsonPropertyName("email")] public string Email { get; set; } = null!;
}
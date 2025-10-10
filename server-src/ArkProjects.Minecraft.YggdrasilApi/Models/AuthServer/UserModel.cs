using System.Text.Json.Serialization;
using ArkProjects.Minecraft.Database.Entities.Users;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class UserModel
{
    [JsonPropertyName("username")] public required string UserName { get; set; }

    [JsonPropertyName("id")] public required Guid Id { get; set; }

    [JsonPropertyName("properties")] public required IReadOnlyList<UserPropertyModel> Properties { get; set; }

    public static UserModel Map(UserEntity user)
    {
        return new UserModel
        {
            Id = user.Id,
            UserName = user.Login,
            Properties = Array.Empty<UserPropertyModel>()
        };
    }
}
using System.Text.Json.Serialization;
using ArkProjects.Minecraft.Database.Entities.Yg;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;

public class UserProfileModel
{
    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("id")] public required Guid Id { get; set; }

    public static UserProfileModel Map(UserProfileEntity profile)
    {
        return new UserProfileModel
        {
            Id = profile.Guid,
            Name = profile.Name
        };
    }
}
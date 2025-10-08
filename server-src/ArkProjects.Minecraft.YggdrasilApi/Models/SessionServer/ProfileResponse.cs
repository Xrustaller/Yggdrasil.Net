using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Models.SessionServer;

public class ProfileResponse
{
    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("id")] public required Guid Id { get; set; }

    /// <summary>
    ///     <see cref="KnownProfileActions" />
    /// </summary>
    [JsonPropertyName("profileActions")]
    public required IReadOnlyList<string> ProfileActions { get; set; }

    [JsonPropertyName("properties")] public required IReadOnlyList<PropertyModel> Properties { get; set; }

    /// <param name="Name">
    ///     <see cref="KnownProfileProperties" />
    /// </param>
    /// <param name="Value"></param>
    /// <param name="Signature"></param>
    public record PropertyModel(string Name, string Value, string? Signature);
}
using System.ComponentModel.DataAnnotations;

namespace ArkProjects.Minecraft.Database.Entities;

public class ServerEntity
{
    [Key]
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset? DeletedAt { get; set; } = null!;

    /// <summary>
    ///     If set then will be used as fallback
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    ///     Yg domain
    /// </summary>
    public string? YgDomain { get; set; } = null!;

    public required List<string> SkinDomains { get; set; } = [];
    public List<string>? UploadableTextures { get; set; } 


    public required string Name { get; set; } = string.Empty;

    public required string HomePageUrl { get; set; } = string.Empty;
    public required string RegisterUrl { get; set; } = string.Empty;

    public required byte[] PfxCert { get; set; } = [];
}
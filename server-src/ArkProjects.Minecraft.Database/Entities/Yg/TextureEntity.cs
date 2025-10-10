using System.ComponentModel.DataAnnotations;

namespace ArkProjects.Minecraft.Database.Entities.Yg;

public class TextureEntity
{
    [Key]
    public Guid Id { get; set; }
    public required string Texture { get; set; }
    public required byte[] File { get; set; }
    public required byte[] Sha256 { get; set; }
}
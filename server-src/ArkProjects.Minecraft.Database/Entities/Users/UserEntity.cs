using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArkProjects.Minecraft.AspShared.EntityFramework;

namespace ArkProjects.Minecraft.Database.Entities.Users;

public class UserEntity : IEntityWithDeletingFlag
{
    [Key]
    public required Guid Id { get; set; }
    public required string Login { get; set; }
    public required string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    [NotMapped] 
    public string LoginNormalized => Login.Normalize().ToUpper();
    
    [NotMapped] 
    public string EmailNormalized => Email.Normalize().ToUpper();
}
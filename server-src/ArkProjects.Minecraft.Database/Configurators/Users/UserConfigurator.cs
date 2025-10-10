using ArkProjects.Minecraft.Database.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArkProjects.Minecraft.Database.Configurators.Users;

public class UserConfigurator : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasIndex(x => new { Guid = x.Id }).IsUnique();
        builder.HasIndex(x => new { x.Login, x.DeletedAt }).IsUnique();
    }
}
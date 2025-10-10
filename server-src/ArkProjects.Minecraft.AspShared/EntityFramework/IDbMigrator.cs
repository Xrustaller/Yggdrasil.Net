using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.AspShared.EntityFramework;

public interface IDbMigrator
{
    DbContext DbContext { get; }
    Task<string[]> GetPendingMigrationsAsync();
    Task MigrateAsync(CancellationToken ct = default);
    Task SeedAsync(CancellationToken ct = default);
}

public interface IDbMigrator<out T> : IDbMigrator where T : DbContext
{
    public new T DbContext { get; }
}
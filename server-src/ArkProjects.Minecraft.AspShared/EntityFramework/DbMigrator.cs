using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArkProjects.Minecraft.AspShared.EntityFramework;

public class DbMigrator<T> : IDbMigrator<T> where T : DbContext
{
    private readonly ILogger<DbMigrator<T>> _logger;
    private readonly IDbSeeder<T>? _seeder;

    public DbMigrator(T db, ILogger<DbMigrator<T>> logger, IDbSeeder<T>? seeder = null)
    {
        _seeder = seeder;
        DbContext = db;
        _logger = logger;
    }

    public T DbContext { get; }

    DbContext IDbMigrator.DbContext => DbContext;

    public async Task<string[]> GetPendingMigrationsAsync()
    {
        return (await DbContext.Database.GetPendingMigrationsAsync()).ToArray();
    }

    public async Task MigrateAsync(CancellationToken ct = default)
    {
        string[] pendingMigrations = await GetPendingMigrationsAsync();
        if (pendingMigrations.Length == 0)
        {
            _logger.LogInformation("No pending migrations");
            return;
        }

        _logger.LogInformation("Begin applying pending migrations {pending}", (object)pendingMigrations);
        await DbContext.Database.MigrateAsync(ct);
        _logger.LogInformation("Successfully migrated");
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (_seeder != null)
        {
            _logger.LogInformation("Begin seeding");
            await _seeder.SeedAsync(DbContext, ct);
            _logger.LogInformation("Successfully seed");
        }
        else
        {
            _logger.LogInformation("Seeder for db not found");
        }
    }
}
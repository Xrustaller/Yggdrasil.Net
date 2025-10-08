using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.Minecraft.AspShared.EntityFramework;

public static class EfStartupMigrationHelper
{
    /// <summary>
    ///     Применяет все ожидающие миграции указанного контекста
    /// </summary>
    public static async Task MigrateAsync<T>(IServiceProvider serviceProvider, CancellationToken ct = default)
        where T : DbContext
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IDbMigrator<T> migrator = scope.ServiceProvider.GetRequiredService<IDbMigrator<T>>();
        await migrator.MigrateAsync(ct);
        await migrator.SeedAsync(ct);
    }
}
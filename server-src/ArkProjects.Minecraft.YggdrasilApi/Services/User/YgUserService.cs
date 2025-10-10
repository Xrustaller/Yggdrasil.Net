using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.Database.Entities.Yg;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.User;

public class YgUserService(McDbContext db) : IYgUserService
{
    public async Task<UserEntity?> GetUserByLoginOrEmailAsync(string loginOrEmail, string domain,
        CancellationToken ct = default)
    {
        string n = loginOrEmail.Normalize().ToUpper();
        UserEntity? user = await db.Users
            .AsNoTracking()
            .Where(x => (x.EmailNormalized == n || x.LoginNormalized == n) && x.DeletedAt == null)
            .FirstOrDefaultAsync(ct);
        return user;
    }

    public async Task<UserEntity?> GetUserByAccessTokenAsync(string accessToken, string domain,
        CancellationToken ct = default)
    {
        UserEntity? user = await db.UserAccessTokens
            .AsNoTracking()
            .Where(x =>
                x.AccessToken == accessToken &&
                x.Server!.YgDomain == domain &&
                x.User!.DeletedAt == null)
            .Select(x => x.User)
            .FirstOrDefaultAsync(ct);
        return user;
    }

    public Task<UserProfileEntity?> GetUserProfileByGuidAsync(Guid profileGuid, string domain, CancellationToken ct = default)
    {
        return GetUserExtendedProfileAsync(profileGuid, null, domain, ct);
    }

    public async Task<string> CreateAccessTokenAsync(string clientToken, Guid userGuid, string domain,
        CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserEntity user = await db.Users
            .Where(x => x.DeletedAt == null && x.Id == userGuid)
            .FirstAsync(ct);
        ServerEntity server = await db.Servers
            .Where(x => x.DeletedAt == null && x.YgDomain == domain)
            .FirstAsync(ct);

        TimeSpan exp = TimeSpan.FromDays(2);
        TimeSpan refr = TimeSpan.FromDays(28);
        UserAccessTokenEntity entity = new()
        {
            Server = server,
            User = user,
            AccessToken = Guid.NewGuid().ToString(),
            ClientToken = clientToken,
            ExpiredAt = now + exp,
            MustBeRefreshedAt = now + refr,
            CreatedAt = now
        };
        db.UserAccessTokens.Add(entity);
        await db.SaveChangesAsync(CancellationToken.None);
        return entity.AccessToken;
    }

    public async Task<bool> ValidateAccessTokenAsync(string? clientToken, string accessToken, string domain,
        CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        bool isValid = await db.UserAccessTokens
            .Where(x =>
                (x.ClientToken == clientToken || clientToken == null) &&
                x.User!.DeletedAt == null &&
                x.AccessToken == accessToken &&
                x.ExpiredAt > now &&
                x.Server!.YgDomain == domain)
            .AnyAsync(ct);
        return isValid;
    }

    public async Task<bool> CanRefreshAccessTokenAsync(string? clientToken, string accessToken, string domain,
        CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        bool isValid = await db.UserAccessTokens
            .Where(x =>
                (x.ClientToken == clientToken || clientToken == null) &&
                x.User!.DeletedAt == null &&
                x.AccessToken == accessToken &&
                x.MustBeRefreshedAt > now &&
                x.Server!.YgDomain == domain)
            .AnyAsync(ct);
        return isValid;
    }

    public async Task InvalidateAccessTokenAsync(Guid userGuid, string accessToken, string domain,
        CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserAccessTokenEntity tokenEntity = await db.UserAccessTokens
            .Where(x =>
                x.User!.Id == userGuid &&
                x.User!.DeletedAt == null &&
                x.AccessToken == accessToken &&
                x.ExpiredAt > now &&
                x.Server!.YgDomain == domain)
            .FirstAsync(ct);
        tokenEntity.ExpiredAt = now;
        tokenEntity.MustBeRefreshedAt = now;
        await db.SaveChangesAsync(CancellationToken.None);
    }

    public async Task InvalidateAllAccessTokensAsync(Guid userGuid, string domain, CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserAccessTokenEntity[] tokenEntities = await db.UserAccessTokens
            .Where(x =>
                x.User!.Id == userGuid &&
                x.User!.DeletedAt == null &&
                x.ExpiredAt > now &&
                x.Server!.YgDomain == domain)
            .ToArrayAsync(ct);
        foreach (UserAccessTokenEntity tokenEntity in tokenEntities)
        {
            tokenEntity.ExpiredAt = now;
            tokenEntity.MustBeRefreshedAt = now;
        }

        await db.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<UserProfileEntity?> GetUserExtendedProfileAsync(Guid? profileGuid, string? profileName,
        string domain,
        CancellationToken ct = default)
    {
        IQueryable<UserProfileEntity> query = db.UserProfiles
            .AsNoTracking()
            .Where(x =>
                x.Server!.YgDomain == domain &&
                x.User!.DeletedAt == null);
        if (profileGuid != null)
            query = query.Where(x => x.Guid == profileGuid.Value);
        else if (!string.IsNullOrWhiteSpace(profileName))
            query = query.Where(x => x.Name == profileName);
        else
            throw new ArgumentNullException("", "guid or name must be not null");

        return await query.FirstOrDefaultAsync(ct);
    }
}
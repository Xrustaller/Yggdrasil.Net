using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.Service;

public class ServiceUsersService(
    McDbContext context,
    IUserPasswordService passwordService) : IServiceUsersService
{
    public async Task<List<UserGetResponse>> GetUsersAsync(CancellationToken ct)
    {
        return await context.Users
            .Select(u => new UserGetResponse(u))
            .ToListAsync(ct);
    }

    public async Task<UserEntity?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<bool> CheckEmailExistAsync(string email, CancellationToken ct)
    {
        return await context.Users.AnyAsync(u => u.EmailNormalized == email.Normalize().ToUpper(), ct);
    }

    public async Task<UserEntity> AddUserAsync(string login, string email, string password, CancellationToken ct)
    {
        UserEntity user = new()
        {
            Id = Guid.NewGuid(),
            Login = login,
            Email = email,
            LoginNormalized = login.Normalize().ToUpper(),
            EmailNormalized = email.Normalize().ToUpper(),
            PasswordHash = passwordService.CreatePasswordHash(password),
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        UserEntity? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return false;
        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserEntity?> UpdateUserAsync(Guid userId, string? newLogin, string? newEmail, string? newPasswordHash, bool setDelete, CancellationToken ct)
    {
        UserEntity? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return null;
        
        if (!string.IsNullOrEmpty(newLogin))
        {
            user.Login = newLogin;
            user.LoginNormalized = newLogin.Normalize().ToUpper();
        }
        
        if (!string.IsNullOrEmpty(newEmail))
        {
            user.Email = newEmail;
            user.EmailNormalized = newEmail.Normalize().ToUpper();
        }
        
        if (!string.IsNullOrEmpty(newPasswordHash))
            user.PasswordHash = newPasswordHash;

        user.DeletedAt = setDelete ? DateTimeOffset.UtcNow : null;

        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
        return user;
    }
}
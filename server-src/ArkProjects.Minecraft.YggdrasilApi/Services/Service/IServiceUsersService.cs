using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.Service;

public interface IServiceUsersService
{
    Task<List<UserGetResponse>> GetUsersAsync(CancellationToken ct);
    Task<UserEntity?> GetUserAsync(Guid userId, CancellationToken ct);
    Task<UserEntity> AddUserAsync(string username, string email, string password, CancellationToken ct);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct);

    Task<bool> CheckEmailExistAsync(string email, CancellationToken ct);
    Task<UserEntity?> UpdateUserAsync(Guid userId, string? newLogin, string? newEmail, string? newPasswordHash, CancellationToken ct);
}
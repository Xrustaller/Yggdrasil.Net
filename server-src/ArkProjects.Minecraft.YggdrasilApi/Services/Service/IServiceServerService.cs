using ArkProjects.Minecraft.Database.Entities;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.Service;

public interface IServiceServerService
{
    Task<List<ServiceEntity>> GetServicesAsync(CancellationToken ct = default);
    Task<ServiceEntity?> GetServiceAsync(string serviceName, CancellationToken ct = default);
    Task<ServiceEntity> CreateServiceAsync(string name, bool createOtherService = false, CancellationToken ct = default);
    Task<bool> DeleteServiceAsync(string name, CancellationToken ct = default);
    Task<ServiceEntity?> UpdateServiceAsync(string name, bool? createOtherService, CancellationToken ct = default);
}
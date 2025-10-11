using System.Security.Cryptography;
using System.Text;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.Service;

public class KeyService(McDbContext context) : IKeyService
{
    public async Task<List<ServiceEntity>> GetServicesAsync(CancellationToken ct = default)
    {
        return await context.Services.ToListAsync(ct);
    }

    public async Task<ServiceEntity?> GetServiceAsync(string serviceName, CancellationToken ct = default)
    {
        return await context.Services.FirstOrDefaultAsync(x => x.Name == serviceName, ct);
    }

    public async Task<ServiceEntity?> CreateServiceAsync(string name, bool createOtherService = false, CancellationToken ct = default)
    {
        if (await context.Services.AnyAsync(x => x.Name == name, ct))
            return null;
        
        string secret = GenerateSecret();
        ServiceEntity service = new()
        {
            Name = name,
            Secret = secret,
            CreateOtherService = createOtherService
        };

        context.Services.Add(service);
        await context.SaveChangesAsync(ct);
        return service;
    }

    public async Task<bool> DeleteServiceAsync(string name, CancellationToken ct = default)
    {
        ServiceEntity? service = await context.Services.FirstOrDefaultAsync(s => s.Name == name, ct);
        if (service == null)
            return false;
        context.Services.Remove(service);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ServiceEntity?> UpdateServiceAsync(string name, bool? createOtherService, CancellationToken ct = default)
    {
        ServiceEntity? service = await context.Services.FirstOrDefaultAsync(s => s.Name == name, ct);
        if (service == null)
            return null;

        if (createOtherService.HasValue)
            service.CreateOtherService = createOtherService.Value;

        context.Services.Update(service);
        await context.SaveChangesAsync(ct);
        return service;
    }

    private static string GenerateSecret()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] keyBytes = new byte[32];
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
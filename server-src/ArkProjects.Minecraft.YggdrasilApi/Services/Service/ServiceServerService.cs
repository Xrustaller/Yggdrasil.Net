using System.Security.Cryptography;
using System.Text;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.Service;

public class ServiceServerService(McDbContext context) : IServiceServerService
{
    public async Task<List<ServiceEntity>> GetServicesAsync(CancellationToken ct = default)
    {
        return await context.Services.ToListAsync(ct);
    }

    public async Task<ServiceEntity?> GetServiceAsync(string serviceName, CancellationToken ct = default)
    {
        return await context.Services.FirstOrDefaultAsync(x => x.Name == serviceName, ct);
    }

    public async Task<ServiceEntity> CreateServiceAsync(string name, bool createOtherService = false, CancellationToken ct = default)
    {
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
        byte[] randomBytes = new byte[32];
        rng.GetBytes(randomBytes);

        string guidPart = Guid.NewGuid().ToString("N");
        string timePart = Convert.ToHexString(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

        string base64 = Convert.ToBase64String(randomBytes)
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "");

        string hashInput = $"{guidPart}{timePart}{base64}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));

        string secretRaw = Convert.ToBase64String(hash)
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "");
            
        int length = Math.Min(48, secretRaw.Length);
        return secretRaw[..length];
    }
}
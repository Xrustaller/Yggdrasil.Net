using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ArkProjects.Minecraft.AspShared.EntityFramework;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.YggdrasilApi.Services.Service;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services;

public class McDbContextSeeder(
    ILogger<McDbContextSeeder> logger, 
    IUserPasswordService passwordService) : IDbSeeder<McDbContext>
{
    public async Task SeedAsync(McDbContext context, CancellationToken ct = default)
    {
        if (!await context.Services.AnyAsync(ct))
        {
            logger.LogInformation("Create default service");
            
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] keyBytes = new byte[32];
            rng.GetBytes(keyBytes);

            string secret = Convert.ToBase64String(keyBytes);
            
            ServiceEntity service = new()
            {
                Name = "admin_service",
                Secret = secret,
                CreateOtherService = true
            };

            context.Services.Add(service);
            await context.SaveChangesAsync(ct);
        }
        
        if (!await context.Servers.AnyAsync(ct))
        {
            logger.LogInformation("Create default server");
            RSA rsaKey = RSA.Create(4096);

            const string ygDomain = "yggdrasil-dev.sv1.in.arkprojects.space";
            CertificateRequest certReq = new($"CN={ygDomain}", rsaKey, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            X509Certificate2 cert = certReq.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));
            byte[] pfxCert = cert.Export(X509ContentType.Pfx);

            ServerEntity server = new()
            {
                Name = "Yggdrasil MC.NET",
                HomePageUrl = "https://yggdrasil.sv1.in.SignOutRequest/home",
                RegisterUrl = "https://yggdrasil.sv1.in.arkprojects.space/register",
                YgDomain = ygDomain,
                Default = true,
                SkinDomains = [ygDomain],
                UploadableTextures = ["CAPE", "SKIN"],
                PfxCert = pfxCert,
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Servers.Add(server);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        //users
        if (!await context.Users.AnyAsync(ct))
        {
            logger.LogInformation("Create admin user");
            const string login = "admin";
            const string email = "admin@test.com";
            Guid guid = Guid.NewGuid();
            string password = guid.ToString();
            UserEntity user = new()
            {
                Id = guid,
                Login = login,
                Email = email,
                LoginNormalized = login.Normalize().ToUpper(),
                EmailNormalized = email.Normalize().ToUpper(),
                PasswordHash = passwordService.CreatePasswordHash(password),
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
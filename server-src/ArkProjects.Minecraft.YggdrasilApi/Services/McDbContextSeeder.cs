using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ArkProjects.Minecraft.AspShared.EntityFramework;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Services;

public class McDbContextSeeder : IDbSeeder<McDbContext>
{
    private readonly ILogger<McDbContextSeeder> _logger;
    private readonly IUserPasswordService _passwordService;

    public McDbContextSeeder(ILogger<McDbContextSeeder> logger, IUserPasswordService passwordService)
    {
        _logger = logger;
        _passwordService = passwordService;
    }

    public async Task SeedAsync(McDbContext db, CancellationToken ct = default)
    {
        if (!await db.Servers.AnyAsync(ct))
        {
            _logger.LogInformation("Create default server");
            RSA rsaKey = RSA.Create(4096);

            string ygDomain = "yggdrasil-dev.sv1.in.arkprojects.space";
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
                SkinDomains = new List<string> { ygDomain },
                UploadableTextures = new List<string> { "CAPE", "SKIN" },
                PfxCert = pfxCert,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Servers.Add(server);
            await db.SaveChangesAsync(CancellationToken.None);
        }

        //users
        if (!await db.Users.AnyAsync(ct))
        {
            _logger.LogInformation("Create admin user");
            string login = "admin";
            string email = "admin@test.com";
            Guid guid = Guid.NewGuid();
            string password = guid.ToString();
            UserEntity user = new()
            {
                Guid = guid,
                Login = login,
                LoginNormalized = login.Normalize().ToUpper(),
                Email = email,
                EmailNormalized = email.Normalize().ToUpper(),
                PasswordHash = _passwordService.CreatePasswordHash(password),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
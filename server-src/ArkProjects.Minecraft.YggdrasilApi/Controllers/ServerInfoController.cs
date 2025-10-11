using System.Security.Cryptography.X509Certificates;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.YggdrasilApi.Models.ServerInfo;
using ArkProjects.Minecraft.YggdrasilApi.Options;
using ArkProjects.Minecraft.YggdrasilApi.Services.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("/")]
public class ServerInfoController(
    //ILogger<ServerInfoController> logger, 
    IYgServerService serverService,
    IOptions<ServerNodeOptions> options) : ControllerBase
{
    private readonly ServerNodeOptions _options = options.Value;

    [HttpGet]
    public async Task<ServerInfoModel> GetInfo(CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        ServerEntity? info = await serverService.GetServerInfoAsync(domain, true, ct);

        return new ServerInfoModel
        {
            SkinDomains = info!.SkinDomains,
            SignaturePublicKey = X509CertificateLoader.LoadPkcs12(info.PfxCert, null)
                .GetRSAPrivateKey()!
                .ExportSubjectPublicKeyInfoPem(),
            Meta = new ServerMetadataModel
            {
                ServerName = info.Name,
                ImplementationName = _options.Implementation,
                ImplementationVersion = _options.Version,
                FeatureNonEmailLogin = true,
                Links = new ServerMetadataLinksModel
                {
                    HomePage = info.HomePageUrl,
                    Register = info.RegisterUrl
                }
            }
        };
    }
}
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.Database.Entities.Yg;
using ArkProjects.Minecraft.YggdrasilApi.Misc;
using ArkProjects.Minecraft.YggdrasilApi.Models.SessionServer;
using ArkProjects.Minecraft.YggdrasilApi.Services.Server;
using ArkProjects.Minecraft.YggdrasilApi.Services.User;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("sessionserver/session/minecraft")]
public class SessionServerController(
    //ILogger<SessionServerController> logger,
    IYgUserService userService,
    IYgServerService serverService)
    : ControllerBase
{
    [HttpPost("join")]
    public async Task Join([FromBody] JoinRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;

        bool isValid = await userService.ValidateAccessTokenAsync(null, req.AccessToken, domain, ct);
        if (!isValid) throw new YgServerException(ErrorResponseFactory.InvalidToken());

        UserProfileEntity? profile = await userService.GetUserProfileByGuidAsync(req.SelectedProfile, domain, ct);
        await serverService.JoinProfileToServer(profile!.Id, req.ServerId, ct);
    }

    [HttpGet("hasJoined")]
    public async Task<HasJoinedResponse> HasJoined([FromQuery] HasJoinedRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        Guid? profileGuid = await serverService.ProfileJoinedToServer(req.UserName, req.ServerId, ct);
        if (profileGuid == null) throw new YgServerException(ErrorResponseFactory.Custom(400, "PROFILE_NOT_JOINED", "Profile not joined"));

        UserProfileEntity? profile = await userService.GetUserProfileByGuidAsync(profileGuid.Value, domain, ct);
        UserExtendedProfileModel extProfile = UserExtendedProfileModel.Map(profile!);
        HtmlString? extProfileJson = new(JsonSerializer.Serialize(extProfile, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        byte[] extProfileBytes = Encoding.UTF8.GetBytes(extProfileJson!.Value!);
        string extProfileB64 = Convert.ToBase64String(extProfileBytes);
        ServerEntity? server = await serverService.GetServerInfoByProfileAsync(extProfile!.ProfileId, ct);

        return new HasJoinedResponse
        {
            Id = extProfile.ProfileId,
            Name = extProfile.ProfileName,
            Properties =
            [
                new HasJoinedResponse.PropertyModel(KnownProfileProperties.Textures, extProfileB64, GetSign(X509CertificateLoader.LoadPkcs12(server!.PfxCert, null), extProfileB64))
            ]
        };
    }

    [HttpGet("profile/{uuid}")]
    public async Task<ProfileResponse> Profile(ProfileRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        UserProfileEntity? profile = await userService.GetUserProfileByGuidAsync(req.UserId, domain, ct);
        if (profile == null) throw new YgServerException(ErrorResponseFactory.Custom(400, "PROFILE_NOT_EXIST", "Profile not exist"));

        UserExtendedProfileModel extProfile = UserExtendedProfileModel.Map(profile);
        HtmlString? extProfileJson = new(JsonSerializer.Serialize(extProfile, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        byte[] extProfileBytes = Encoding.UTF8.GetBytes(extProfileJson!.Value!);
        string extProfileB64 = Convert.ToBase64String(extProfileBytes);

        ServerEntity? server = await serverService.GetServerInfoByProfileAsync(profile.Guid, ct);

        return new ProfileResponse
        {
            Id = profile.Guid,
            Name = profile.Name,
            ProfileActions = [],
            Properties =
            [
                new ProfileResponse.PropertyModel(KnownProfileProperties.Textures, extProfileB64, req.Unsigned
                    ? null
                    : GetSign(X509CertificateLoader.LoadPkcs12(server!.PfxCert, null), extProfileB64)),
                new ProfileResponse.PropertyModel(KnownProfileProperties.UploadableTextures,
                    string.Join(',', server!.UploadableTextures ?? []), null)
            ]
        };
    }

    private static string GetSign(X509Certificate2 cert, string text)
    {
        byte[] sign = cert
            .GetRSAPrivateKey()!
            .SignData(Encoding.UTF8.GetBytes(text), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(sign);
    }
}
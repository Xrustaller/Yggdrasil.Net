using System.Security.Cryptography;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.Database.Entities.Yg;
using ArkProjects.Minecraft.YggdrasilApi.Misc;
using ArkProjects.Minecraft.YggdrasilApi.Models.SessionServer;
using ArkProjects.Minecraft.YggdrasilApi.Models.UserProfile;
using ArkProjects.Minecraft.YggdrasilApi.Services.Server;
using ArkProjects.Minecraft.YggdrasilApi.Services.User;
using MetadataExtractor.Formats.Png;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Directory = MetadataExtractor.Directory;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("/api/user/profile")]
public class UserProfileController(
    ILogger<UserProfileController> logger,
    IYgServerService serverService,
    IYgUserService userService,
    McDbContext db)
    : ControllerBase
{
    //TODO rewrite
    [HttpPut("{uuid}/skin")]
    public async Task<ActionResult> UploadSkin(UploadSkinRequest req, string uuid, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        string? accessToken = GetAccessToken();
        if (accessToken == null) throw new YgServerException(ErrorResponseFactory.InvalidToken());

        bool isValid = await userService.ValidateAccessTokenAsync(null, accessToken, domain, ct);
        if (!isValid) throw new YgServerException(ErrorResponseFactory.InvalidToken());

        if (req.File.Length > 1024 * 300) throw new YgServerException(ErrorResponseFactory.Custom(400, "FILE_SIZE_LIMIT", "Max file size is 300Kb"));

        await ApplyTextureAsync(req, ct);

        return Ok();
    }

    private async Task ApplyTextureAsync(UploadSkinRequest req, CancellationToken ct = default)
    {
        ServerEntity? server = await serverService.GetServerInfoByProfileAsync(req.UserId, ct);

        byte[] imageBytes = ReadFile(req.File);
        (int width, int height) dims = ReadDimensions(imageBytes);
        byte[] sha256 = SHA256.HashData(imageBytes);
        string texture = dims switch
        {
            (64, 64) => ProfileTextureModel.SkinTextureName,
            (32, 64) => ProfileTextureModel.CapeTextureName,
            _ => throw new NotSupportedException("File is not supported")
        };
        TextureEntity textureEntity = new()
        {
            File = imageBytes,
            Guid = Guid.NewGuid(),
            Sha256 = sha256,
            Texture = texture
        };
        db.Textures.Add(textureEntity);
        string url = $"https://{server!.YgDomain}/api/texture/{textureEntity.Guid}";
        UserProfileEntity userProfileEntity = await db.UserProfiles.SingleAsync(x => x.Guid == req.UserId, ct);
        switch (textureEntity.Texture)
        {
            case ProfileTextureModel.SkinTextureName:
                userProfileEntity.SkinFileUrl = url;
                break;
            case ProfileTextureModel.CapeTextureName:
                userProfileEntity.CapeFileUrl = url;
                break;
        }

        await db.SaveChangesAsync(CancellationToken.None);
    }

    private static byte[] ReadFile(IFormFile file)
    {
        using Stream stream = file.OpenReadStream();
        byte[] bytes = new byte[file.Length];
        int read = stream.Read(bytes);
        if (read != bytes.Length) 
            throw new Exception("Cant read full img to mem");

        return bytes;
    }

    private static (int width, int height) ReadDimensions(byte[] file)
    {
        using MemoryStream stream = new(file);
        IReadOnlyList<Directory> pngMeta = PngMetadataReader.ReadMetadata(stream);
        PngDirectory ihdr = pngMeta
            .OfType<PngDirectory>()
            .First(x => x.GetPngChunkType() == PngChunkType.IHDR);
        int? width = ihdr.GetObject(PngDirectory.TagImageWidth) as int?;
        int? height = ihdr.GetObject(PngDirectory.TagImageHeight) as int?;
        if (width is null or > 64) throw new NotSupportedException("Image width must be >= 1024");

        if (height is null or > 64) throw new NotSupportedException("Image height must be >= 1024");

        return (width.Value, height.Value);
    }

    private string? GetAccessToken()
    {
        string auth = Request.Headers.Authorization.ToString();
        if (auth.Length < "Bearer ".Length)
            return null;

        string[] parts = auth.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts is not ["Bearer", _]
            ? null
            : parts[1];
    }
}
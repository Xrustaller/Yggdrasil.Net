using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities.Yg;
using ArkProjects.Minecraft.YggdrasilApi.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("/api/texture")]
public class TextureController(McDbContext db) : ControllerBase
{
    [HttpGet("{texGuid:guid}")]
    public async Task<ActionResult> GetTexture([FromRoute] Guid texGuid, CancellationToken ct = default)
    {
        TextureEntity? tex = await db.Textures.FirstOrDefaultAsync(x => x.Id == texGuid, ct);
        return tex == null
            ? throw new YgServerException(ErrorResponseFactory.Custom(404, "TEXTURE_NOT_FOUND", "Texture not found"))
            : File(tex.File, "image/png");
    }
}
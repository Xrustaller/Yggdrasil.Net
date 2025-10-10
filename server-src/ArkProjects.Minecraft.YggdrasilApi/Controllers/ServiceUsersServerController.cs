using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.YggdrasilApi.Filters;
using ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;
using ArkProjects.Minecraft.YggdrasilApi.Services.Service;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("service/users")]
[ServiceAuth]
public class ServiceUsersServerController(
    ILogger<ServiceUsersServerController> logger,
    IServiceUsersService usersService,
    IUserPasswordService passwordService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UsersGetResponse>> GetList(CancellationToken ct = default)
    {
        return Ok(new UsersGetResponse(await usersService.GetUsersAsync(ct)));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserGetResponse>> Get([FromRoute] Guid userId, CancellationToken ct = default)
    {
        UserEntity? user = await usersService.GetUserAsync(userId, ct);
        if (user == null)
            return NotFound(new { detail = "User not found" });
        return Ok(new UserGetResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserNewResponse>> New([FromBody] UserNewRequest req, CancellationToken ct = default)
    {
        if (!passwordService.CheckPasswordRequirements(req.Password))
            return BadRequest(new { detail = "The password must be between 6 and 30 characters" });

        if (await usersService.CheckEmailExistAsync(req.Email, ct))
            return Conflict(new { detail = "User already exists" });

        UserEntity user = await usersService.AddUserAsync(req.Login, req.Email, req.Password, ct);

        logger.LogInformation($"Create new user {req.Email}");
        return Created($"service/users/{user.Id}", new UserNewResponse(user));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult> Remove([FromRoute] Guid userId, CancellationToken ct = default)
    {
        if (!await usersService.DeleteUserAsync(userId, ct))
            return NotFound(new { detail = "User not found" });
        logger.LogInformation($"Deleted user {userId}");
        return NoContent();
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserPutResponse>> Put([FromRoute] Guid userId, [FromBody] UserPutRequest req, CancellationToken ct = default)
    {
        string? newLogin = null;
        string? newEmail = null;
        string? newPassword = null;

        if (!string.IsNullOrWhiteSpace(req.Login))
            newLogin = req.Login;

        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            if (await usersService.CheckEmailExistAsync(req.Email, ct))
                return Conflict(new { detail = "Email already used" });
            newEmail = req.Email;
        }

        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            if (!passwordService.CheckPasswordRequirements(req.Password))
                return BadRequest(new { detail = "The password must be between 6 and 30 characters" });
            newPassword = passwordService.CreatePasswordHash(req.Password);
        }

        UserEntity? user = await usersService.UpdateUserAsync(userId, newLogin, newEmail, newPassword, req.SetDelete, ct);
        if (user == null)
            return NotFound(new { detail = "User not found" });

        logger.LogInformation($"Updated user {user.Email}");
        return Ok(new UserPutResponse(user));
    }
}
using ArkProjects.Minecraft.Database.Entities.Users;
using ArkProjects.Minecraft.Database.Entities.Yg;
using ArkProjects.Minecraft.YggdrasilApi.Misc;
using ArkProjects.Minecraft.YggdrasilApi.Models.AuthServer;
using ArkProjects.Minecraft.YggdrasilApi.Services.User;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.AspNetCore.Mvc;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("authserver")]
public class AuthServerController(
    ILogger<AuthServerController> logger,
    IYgUserService userService,
    IUserPasswordService passwordService) : ControllerBase
{
    [HttpPost("authenticate")]
    public async Task<AuthenticateResponse> Authenticate([FromBody] AuthenticateRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        UserEntity? user = await userService.GetUserByLoginOrEmailAsync(req.LoginOrEmail, domain, ct);
        if (user == null || !passwordService.Validate(req.Password, user.PasswordHash)) throw new YgServerException(ErrorResponseFactory.InvalidCredentials());

        UserProfileEntity? profile = await userService.GetUserProfileByGuidAsync(user.Guid, domain, ct);
        if (profile == null) throw new YgServerException(ErrorResponseFactory.Custom(400, "PROFILE_NOT_EXIST", "Profile not exist"));

        UserProfileModel profileModel = UserProfileModel.Map(profile);

        string clientToken = req.ClientToken ?? Guid.NewGuid().ToString("N");
        string accessToken = await userService.CreateAccessTokenAsync(clientToken, user.Guid, domain, ct);

        return new AuthenticateResponse
        {
            ClientToken = clientToken,
            AccessToken = accessToken,
            AvailableProfiles = [profileModel],
            SelectedProfile = profileModel,
            User = req.RequestUser
                ? new UserModel
                {
                    Id = user.Guid,
                    UserName = user.Login,
                    Properties = []
                }
                : null
        };
    }

    [HttpPost("refresh")]
    public async Task<RefreshResponse> Refresh([FromBody] RefreshRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        bool isValid = await userService.CanRefreshAccessTokenAsync(req.ClientToken, req.AccessToken, domain, ct);
        if (!isValid)
            throw new YgServerException(ErrorResponseFactory.InvalidToken());

        UserEntity? user = await userService.GetUserByAccessTokenAsync(req.AccessToken, domain, ct);
        UserProfileEntity? profile = await userService.GetUserProfileByGuidAsync(user!.Guid, domain, ct);
        string newAccessToken = await userService.CreateAccessTokenAsync(req.ClientToken, user.Guid, domain, ct);

        // исправлено: инвалидируем старый токен, а не новый
        await userService.InvalidateAccessTokenAsync(user.Guid, req.AccessToken, domain, ct);

        return new RefreshResponse
        {
            SelectedProfile = UserProfileModel.Map(profile!),
            AccessToken = newAccessToken,
            ClientToken = req.ClientToken,
            User = req.RequestUser ? UserModel.Map(user) : null
        };
    }

    [HttpPost("validate")]
    public async Task Validate([FromBody] ValidateRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;
        bool isValid = await userService.ValidateAccessTokenAsync(req.ClientToken, req.AccessToken, domain, ct);
        Response.StatusCode = isValid ? 204 : throw new YgServerException(ErrorResponseFactory.InvalidToken());
    }

    [HttpPost("invalidate")]
    public async Task Invalidate([FromBody] InvalidateRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;

        // добавлена валидация токена клиента
        bool isValid = await userService.ValidateAccessTokenAsync(req.ClientToken, req.AccessToken, domain, ct);
        if (!isValid)
            throw new YgServerException(ErrorResponseFactory.InvalidToken());

        // находим пользователя по токену
        UserEntity? user = await userService.GetUserByAccessTokenAsync(req.AccessToken, domain, ct);
        if (user == null)
            throw new YgServerException(ErrorResponseFactory.InvalidToken());

        // инвалидируем конкретный accessToken
        await userService.InvalidateAccessTokenAsync(user.Guid, req.AccessToken, domain, ct);

        // выставляем 204
        Response.StatusCode = 204;
    }

    [HttpPost("signout")]
    public async Task SignOut([FromBody] SignOutRequest req, CancellationToken ct = default)
    {
        string domain = HttpContext.Request.Host.Host;

        // добавлен поиск пользователя по логину/почте
        UserEntity? user = await userService.GetUserByLoginOrEmailAsync(req.UserName, domain, ct);
        if (user == null)
            throw new YgServerException(ErrorResponseFactory.InvalidCredentials());

        // добавлена проверка пароля
        if (!passwordService.Validate(req.Password, user.PasswordHash))
            throw new YgServerException(ErrorResponseFactory.InvalidCredentials());

        // инвалидируем все токены пользователя
        await userService.InvalidateAllAccessTokensAsync(user.Guid, domain, ct);

        // выставляем 204
        Response.StatusCode = 204;
    }
}
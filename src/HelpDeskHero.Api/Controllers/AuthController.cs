using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Security;
using HelpDeskHero.Shared.Contracts.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController
    : ControllerBase
{
    private readonly ITokenService
        _tokenService;

    private readonly RefreshTokenService
        _refreshTokenService;

    private readonly UserManager<ApplicationUser>
        _userManager;

    public AuthController(
        ITokenService tokenService,
        RefreshTokenService refreshTokenService,
        UserManager<ApplicationUser> userManager)
    {
        _tokenService =
            tokenService;

        _refreshTokenService =
            refreshTokenService;

        _userManager =
            userManager;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto dto,
        CancellationToken ct)
    {
        var user =
            await _userManager
                .FindByNameAsync(
                    dto.UserName);

        if (user is null
            || !user.IsActive
            || !await _userManager.CheckPasswordAsync(
                user,
                dto.Password))
        {
            return Unauthorized();
        }

        return Ok(
            await CreateLoginResponseAsync(
                user,
                dto.DeviceName,
                ct));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseDto>>
        Refresh(
            RefreshTokenRequestDto dto,
            CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(
                dto.RefreshToken))
        {
            return Unauthorized();
        }

        var existingToken =
            await _refreshTokenService
                .GetActiveByRawTokenAsync(
                    dto.RefreshToken,
                    ct);

        if (existingToken?.User is null
            || !existingToken.User.IsActive)
        {
            return Unauthorized();
        }

        await _refreshTokenService
            .RevokeAsync(
                existingToken,
                ct);

        var (accessToken, accessExpiresAtUtc) =
            await _tokenService
                .CreateAccessTokenAsync(
                    existingToken.User);

        var (refreshToken, refreshExpiresAtUtc) =
            await _refreshTokenService
                .CreateAsync(
                    existingToken.UserId,
                    dto.DeviceName,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ct);

        return Ok(
            new RefreshTokenResponseDto
            {
                AccessToken =
                    accessToken,

                Token =
                    accessToken,

                RefreshToken =
                    refreshToken,

                AccessTokenExpiresAtUtc =
                    accessExpiresAtUtc,

                ExpiresAtUtc =
                    accessExpiresAtUtc,

                RefreshTokenExpiresAtUtc =
                    refreshExpiresAtUtc
            });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        RefreshTokenRequestDto dto,
        CancellationToken ct)
    {
        var refreshToken =
            await _refreshTokenService
                .GetActiveByRawTokenAsync(
                    dto.RefreshToken,
                    ct);

        if (refreshToken is not null)
        {
            await _refreshTokenService
                .RevokeAsync(
                    refreshToken,
                    ct);
        }

        return Ok();
    }

    private async Task<LoginResponseDto>
        CreateLoginResponseAsync(
            ApplicationUser user,
            string deviceName,
            CancellationToken ct)
    {
        var (accessToken, accessExpiresAtUtc) =
            await _tokenService
                .CreateAccessTokenAsync(
                    user);

        var (refreshToken, refreshExpiresAtUtc) =
            await _refreshTokenService
                .CreateAsync(
                    user.Id,
                    deviceName,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ct);

        var roles =
            (await _userManager.GetRolesAsync(
                user))
            .ToArray();

        return new LoginResponseDto
        {
            AccessToken =
                accessToken,

            Token =
                accessToken,

            RefreshToken =
                refreshToken,

            AccessTokenExpiresAtUtc =
                accessExpiresAtUtc,

            ExpiresAtUtc =
                accessExpiresAtUtc,

            RefreshTokenExpiresAtUtc =
                refreshExpiresAtUtc,

            UserName =
                user.UserName
                ?? "",

            DisplayName =
                user.DisplayName,

            Role =
                roles.FirstOrDefault()
                ?? user.Role,

            Roles =
                roles
        };
    }
}

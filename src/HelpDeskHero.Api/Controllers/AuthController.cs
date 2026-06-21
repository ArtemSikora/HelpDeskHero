using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Security;
using HelpDeskHero.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController
    : ControllerBase
{
    private readonly ITokenService
        _tokenService;

    public AuthController(
        ITokenService tokenService)
    {
        _tokenService =
            tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponseDto> Login(
        LoginRequestDto dto)
    {
        var user =
            GetUser(dto);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(
            CreateLoginResponse(
                user));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public ActionResult<RefreshTokenResponseDto>
        Refresh(
            RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(
                dto.RefreshToken))
        {
            return Unauthorized();
        }

        var user =
            new AppUser
            {
                Id = 1,
                UserName = "admin",
                Role = "Admin"
            };

        var accessToken =
            _tokenService
                .CreateAccessToken(
                    user);

        var refreshToken =
            _tokenService
                .CreateRefreshToken(
                    user);

        return Ok(
            new RefreshTokenResponseDto
            {
                Token =
                    accessToken,

                RefreshToken =
                    refreshToken.Token,

                ExpiresAtUtc =
                    refreshToken.ExpiresAtUtc
            });
    }

    [HttpPost("revoke")]
    [Authorize]
    public IActionResult Revoke()
    {
        return Ok();
    }

    private LoginResponseDto
        CreateLoginResponse(
            AppUser user)
    {
        var accessToken =
            _tokenService
                .CreateAccessToken(
                    user);

        var refreshToken =
            _tokenService
                .CreateRefreshToken(
                    user);

        return new LoginResponseDto
        {
            Token =
                accessToken,

            RefreshToken =
                refreshToken.Token,

            ExpiresAtUtc =
                refreshToken.ExpiresAtUtc,

            UserName =
                user.UserName,

            Role =
                user.Role
        };
    }

    private static AppUser?
        GetUser(
            LoginRequestDto dto)
    {
        if (dto.UserName == "admin"
            && dto.Password == "Admin123!")
        {
            return new AppUser
            {
                Id = 1,
                UserName = "admin",
                Role = "Admin"
            };
        }

        if (dto.UserName == "agent"
            && dto.Password == "Agent123!")
        {
            return new AppUser
            {
                Id = 2,
                UserName = "agent",
                Role = "Agent"
            };
        }

        if (dto.UserName == "user"
            && dto.Password == "User123!")
        {
            return new AppUser
            {
                Id = 3,
                UserName = "user",
                Role = "User"
            };
        }

        return null;
    }
}
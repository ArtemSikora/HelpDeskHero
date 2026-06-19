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
    private readonly ITokenService _tokenService;

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

        var token =
            _tokenService
                .CreateAccessToken(
                    user);

        return Ok(
            new LoginResponseDto
            {
                Token =
                    token,

                ExpiresAtUtc =
                    DateTime.UtcNow
                        .AddMinutes(15),

                UserName =
                    user.UserName,

                Role =
                    user.Role
            });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public ActionResult<RefreshTokenResponseDto> Refresh(
        RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(
                dto.RefreshToken))
        {
            return Unauthorized();
        }

        return Ok(
            new RefreshTokenResponseDto
            {
                Token =
                    Guid.NewGuid()
                        .ToString(),

                ExpiresAtUtc =
                    DateTime.UtcNow
                        .AddMinutes(15),

                RefreshToken =
                    Guid.NewGuid()
                        .ToString()
            });
    }

    [HttpPost("revoke")]
    [Authorize]
    public IActionResult Revoke()
    {
        return Ok(
            new
            {
                Message =
                    "Refresh token revoked"
            });
    }

    private static AppUser? GetUser(
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
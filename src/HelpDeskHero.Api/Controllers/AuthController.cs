using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDeskHero.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponseDto> Login(
        LoginRequestDto dto)
    {
        if (!IsValidUser(dto))
        {
            return Unauthorized();
        }

        var role =
            dto.UserName.Equals(
                "admin",
                StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : "User";

        var (token, expiresAtUtc) =
            CreateToken(
                dto.UserName,
                role);

        return Ok(
            new LoginResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                UserName = dto.UserName,
                Role = role
            });
    }

    private static bool IsValidUser(
        LoginRequestDto dto)
    {
        return
            (dto.UserName == "admin"
             && dto.Password == "Admin123!")

            ||

            (dto.UserName == "user"
             && dto.Password == "User123!");
    }

    private (string Token, DateTime ExpiresAtUtc)
        CreateToken(
            string userName,
            string role)
    {
        var jwt =
            _configuration.GetSection("Jwt");

        var key =
            jwt["Key"]
            ?? throw new InvalidOperationException();

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var expires =
            DateTime.UtcNow.AddHours(8);

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.Name,
                    userName),

                new(
                    ClaimTypes.Role,
                    role)
            };

        var token =
            new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

        return (
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expires);
    }
}
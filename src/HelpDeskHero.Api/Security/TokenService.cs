using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HelpDeskHero.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDeskHero.Api.Security;

public sealed class TokenService
    : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(
        IOptions<JwtOptions> jwt)
    {
        _jwt =
            jwt.Value;
    }

    public string CreateAccessToken(
        AppUser user)
    {
        var expires =
            DateTime.UtcNow
                .AddMinutes(
                    _jwt.AccessTokenMinutes);

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.Name,
                    user.UserName),

                new(
                    ClaimTypes.Role,
                    user.Role)
            };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _jwt.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer:
                    _jwt.Issuer,

                audience:
                    _jwt.Audience,

                claims:
                    claims,

                expires:
                    expires,

                signingCredentials:
                    credentials);

        return
            new JwtSecurityTokenHandler()
                .WriteToken(
                    token);
    }

    public RefreshToken CreateRefreshToken(
        AppUser user)
    {
        var bytes =
            RandomNumberGenerator
                .GetBytes(64);

        return new RefreshToken
        {
            Token =
                Convert
                    .ToBase64String(
                        bytes),

            AppUserId =
                user.Id,

            ExpiresAtUtc =
                DateTime.UtcNow
                    .AddDays(
                        _jwt.RefreshTokenDays)
        };
    }
}
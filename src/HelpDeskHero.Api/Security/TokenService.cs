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
        ApplicationUser user)
    {
        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.Name,
                    user.UserName ?? ""),

                new(
                    ClaimTypes.Role,
                    user.Role ?? "User")
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
                    DateTime.UtcNow
                        .AddMinutes(
                            _jwt.AccessTokenMinutes),

                signingCredentials:
                    credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(
                token);
    }

    public RefreshToken CreateRefreshToken(
        ApplicationUser user)
    {
        return new RefreshToken
        {
            TokenHash =
                Convert.ToBase64String(
                    RandomNumberGenerator
                        .GetBytes(
                            64)),

            ExpiresAtUtc =
                DateTime.UtcNow
                    .AddDays(
                        _jwt.RefreshTokenDays)
        };
    }
}
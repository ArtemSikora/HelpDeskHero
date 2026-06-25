using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using HelpDeskHero.Api.Domain;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDeskHero.Api.Security;

public sealed class TokenService
    : ITokenService
{
    private readonly JwtOptions _jwt;

    private readonly UserManager<ApplicationUser>
        _userManager;

    public TokenService(
        IOptions<JwtOptions> jwt,
        UserManager<ApplicationUser> userManager)
    {
        _jwt =
            jwt.Value;

        _userManager =
            userManager;
    }

    public async Task<(string token, DateTime expiresAtUtc)> CreateAccessTokenAsync(
        ApplicationUser user)
    {
        var roles =
            await _userManager
                .GetRolesAsync(
                    user);

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id),

                new(
                    ClaimTypes.Name,
                    user.UserName ?? ""),

                new(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new(
                    "display_name",
                    user.DisplayName)
            };

        claims.AddRange(
            roles.Select(
                role => new Claim(
                    ClaimTypes.Role,
                    role)));

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

                notBefore:
                    DateTime.UtcNow,

                expires:
                    DateTime.UtcNow.AddMinutes(
                        _jwt.AccessTokenMinutes),

                signingCredentials:
                    credentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
            .WriteToken(
                token);

        return (
            tokenValue,
            token.ValidTo);
    }

    public (string rawToken, string tokenHash) CreateRefreshToken()
    {
        var rawToken =
            Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(
                    64));

        return (
            rawToken,
            ComputeRefreshTokenHash(
                rawToken));
    }

    public string ComputeRefreshTokenHash(
        string rawToken)
    {
        var bytes =
            Encoding.UTF8.GetBytes(
                rawToken);

        var hash =
            SHA256.HashData(
                bytes);

        return Convert.ToHexString(
            hash);
    }
}

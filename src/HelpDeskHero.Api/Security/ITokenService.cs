using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Security;

public interface ITokenService
{
    Task<(string token, DateTime expiresAtUtc)> CreateAccessTokenAsync(
        ApplicationUser user);

    (string rawToken, string tokenHash) CreateRefreshToken();

    string ComputeRefreshTokenHash(
        string rawToken);
}

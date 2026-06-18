using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Security;

public interface ITokenService
{
    string CreateAccessToken(
        AppUser user);

    RefreshToken CreateRefreshToken(
        AppUser user);
}
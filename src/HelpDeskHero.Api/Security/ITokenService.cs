using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Security;

public interface ITokenService
{
    string CreateAccessToken(
        ApplicationUser user);

    RefreshToken CreateRefreshToken(
        ApplicationUser user);
}
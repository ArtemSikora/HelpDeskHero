namespace HelpDeskHero.Api.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; }
        = "";

    public string Audience { get; set; }
        = "";

    public string Key { get; set; }
        = "";

    public int AccessTokenMinutes { get; set; }
        = 15;

    public int RefreshTokenDays { get; set; }
        = 7;
}
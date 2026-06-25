namespace HelpDeskHero.Shared.Contracts.Auth;

public sealed class RefreshTokenResponseDto
{
    public string AccessToken { get; set; }
        = "";

    public string Token { get; set; }
        = "";

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; }
        = "";

    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}

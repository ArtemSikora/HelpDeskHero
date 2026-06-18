namespace HelpDeskHero.Shared.Contracts.Auth;

public sealed class RefreshTokenResponseDto
{
    public string Token { get; set; }
        = "";

    public DateTime ExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; }
        = "";
}
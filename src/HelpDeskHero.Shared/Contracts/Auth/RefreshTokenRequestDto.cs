namespace HelpDeskHero.Shared.Contracts.Auth;

public sealed class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }
        = "";

    public string DeviceName { get; set; }
        = "Unknown";
}

namespace HelpDeskHero.Api.Domain;

public sealed class RefreshToken
{
    public int Id { get; set; }

    public string TokenHash { get; set; }
        = "";

    public DateTime ExpiresAtUtc { get; set; }

    public bool Revoked { get; set; }
}
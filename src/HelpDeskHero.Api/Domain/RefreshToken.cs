namespace HelpDeskHero.Api.Domain;

public sealed class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; }
        = "";

    public DateTime ExpiresAtUtc { get; set; }

    public bool Revoked { get; set; }

    public int AppUserId { get; set; }

    public AppUser? AppUser { get; set; }
}
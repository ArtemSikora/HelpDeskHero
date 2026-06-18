namespace HelpDeskHero.Api.Domain;

public sealed class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; }
        = "";

    public string PasswordHash { get; set; }
        = "";

    public string Role { get; set; }
        = "User";

    public bool IsActive { get; set; }
        = true;

    public List<RefreshToken> RefreshTokens { get; set; }
        = [];
}
using Microsoft.AspNetCore.Identity;

namespace HelpDeskHero.Api.Domain;

public sealed class ApplicationUser
    : IdentityUser
{
    public string Role { get; set; }
        = "User";

    public string DisplayName { get; set; }
        = "";

    public bool IsActive { get; set; }
        = true;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}
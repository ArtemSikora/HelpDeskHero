namespace HelpDeskHero.Api.Domain;

public sealed class AuditLog
{
    public long Id { get; set; }

    public string Action { get; set; }
        = "";

    public string EntityName { get; set; }
        = "";

    public string EntityId { get; set; }
        = "";

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }

    public string? DetailsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}

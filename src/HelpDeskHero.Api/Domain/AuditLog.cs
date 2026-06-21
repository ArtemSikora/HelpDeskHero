namespace HelpDeskHero.Api.Domain;

public sealed class AuditLog
{
    public int Id { get; set; }

    public string Action { get; set; }
        = "";

    public string EntityName { get; set; }
        = "";

    public string EntityId { get; set; }
        = "";

    public string UserName { get; set; }
        = "";

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}
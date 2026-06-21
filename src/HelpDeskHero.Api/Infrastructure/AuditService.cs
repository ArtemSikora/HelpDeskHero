using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Infrastructure;

public sealed class AuditService
{
    private readonly AppDbContext _db;

    public AuditService(
        AppDbContext db)
    {
        _db =
            db;
    }

    public async Task WriteAsync(
        string action,
        string entityName,
        string entityId,
        string userName)
    {
        _db.AuditLogs.Add(
            new AuditLog
            {
                Action =
                    action,

                EntityName =
                    entityName,

                EntityId =
                    entityId,

                UserName =
                    userName,

                CreatedAtUtc =
                    DateTime.UtcNow
            });

        await _db
            .SaveChangesAsync();
    }
}
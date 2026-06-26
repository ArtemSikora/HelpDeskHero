using HelpDeskHero.Api.Infrastructure;
using HelpDeskHero.Shared.Contracts.Audit;
using HelpDeskHero.Shared.Contracts.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public sealed class AuditController
    : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        CancellationToken ct = default)
    {
        pageNumber =
            Math.Max(
                pageNumber,
                1);

        pageSize =
            Math.Clamp(
                pageSize,
                1,
                100);

        var query =
            _db.AuditLogs
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(
                entityName))
        {
            query =
                query.Where(
                    x => x.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(
                action))
        {
            query =
                query.Where(
                    x => x.Action == action);
        }

        var totalCount =
            await query.CountAsync(
                ct);

        var logs =
            await query
                .OrderByDescending(
                    x => x.CreatedAtUtc)
                .Skip(
                    (pageNumber - 1) * pageSize)
                .Take(
                    pageSize)
                .Select(
                    x => new AuditLogDto
                    {
                        Id = x.Id,
                        CreatedAtUtc = x.CreatedAtUtc,
                        Action = x.Action,
                        EntityName = x.EntityName,
                        EntityId = x.EntityId,
                        UserId = x.UserId,
                        UserName = x.UserName,
                        IpAddress = x.IpAddress,
                        DetailsJson = x.DetailsJson
                    })
                .ToListAsync(
                    ct);

        return Ok(
            new PagedResultDto<AuditLogDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = logs
            });
    }
}

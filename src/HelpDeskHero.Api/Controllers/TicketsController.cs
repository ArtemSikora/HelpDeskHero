using System.Security.Claims;

using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure;

using HelpDeskHero.Shared.Contracts.Common;
using HelpDeskHero.Shared.Contracts.Tickets;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TicketsController
    : ControllerBase
{
    private readonly AppDbContext _db;

    private readonly AuditService
        _audit;

    public TicketsController(
        AppDbContext db,
        AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketDto>>>
        GetAll(
            [FromQuery] TicketQueryDto query,
            CancellationToken ct)
    {
        var pageNumber =
            Math.Max(
                query.PageNumber,
                1);

        var pageSize =
            Math.Clamp(
                query.PageSize,
                1,
                100);

        var ticketsQuery =
            _db.Tickets
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(
                query.Search))
        {
            var search =
                query.Search.Trim();

            ticketsQuery =
                ticketsQuery.Where(
                    x =>
                        x.Number.Contains(
                            search)
                        || x.Title.Contains(
                            search)
                        || x.Description.Contains(
                            search));
        }

        if (!string.IsNullOrWhiteSpace(
                query.Status)
            && !string.Equals(
                query.Status,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            ticketsQuery =
                ticketsQuery.Where(
                    x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(
                query.Priority)
            && !string.Equals(
                query.Priority,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            ticketsQuery =
                ticketsQuery.Where(
                    x => x.Priority == query.Priority);
        }

        ticketsQuery =
            ApplySort(
                ticketsQuery,
                query.SortBy,
                query.Desc);

        var totalCount =
            await ticketsQuery
                .CountAsync(
                    ct);

        var tickets =
            await ticketsQuery
                .Skip(
                    (pageNumber - 1) * pageSize)
                .Take(
                    pageSize)
                .ToListAsync(
                    ct);

        return Ok(
            new PagedResultDto<TicketDto>
            {
                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalCount =
                    totalCount,

                Items =
                    tickets.Select(
                            ToDto)
                        .ToList()
            });
    }

    [HttpPost]
    [Authorize(
        Policy =
            "CanManageTickets")]
    public async Task<ActionResult<TicketDto>>
        Create(
            CreateTicketDto dto,
            CancellationToken ct)
    {
        var ticket =
            new Ticket
            {
                Number =
                    $"HDH-{(await _db.Tickets.CountAsync() + 1):0000}",

                Title =
                    dto.Title,

                Description =
                    dto.Description,

                Priority =
                    dto.Priority,

                Status =
                    "New",

                IsDeleted =
                    false,

                CreatedAtUtc =
                    DateTime.UtcNow,

                RowVersion =
                    Guid.NewGuid()
            };

        _db.Tickets
            .Add(
                ticket);

        await _db
            .SaveChangesAsync(
                ct);

        await _audit
            .WriteAsync(
                "CREATE",
                "Ticket",
                ticket.Id.ToString(),
                User.Identity?.Name
                ?? "unknown");

        return Ok(
            ToDto(
                ticket));
    }

    [HttpPut("{id}")]
    [Authorize(
        Policy =
            "AgentOrAdmin")]
    public async Task<IActionResult>
        Update(
            int id,
            UpdateTicketDto dto,
            CancellationToken ct)
    {
        var ticket =
            await _db.Tickets
                .FirstOrDefaultAsync(
                    x =>
                        x.Id
                        == id,
                    ct);

        if (ticket is null)
        {
            return NotFound();
        }

        if (!TryDecodeRowVersion(
                dto.RowVersionBase64,
                out var rowVersion))
        {
            return BadRequest(
                "Invalid row version.");
        }

        _db.Entry(
            ticket)
            .Property(
                x => x.RowVersion)
            .OriginalValue =
                rowVersion;

        ticket.Title =
            dto.Title;

        ticket.Description =
            dto.Description;

        ticket.Priority =
            dto.Priority;

        ticket.Status =
            dto.Status;

        ticket.UpdatedAtUtc =
            DateTime.UtcNow;

        ticket.RowVersion =
            Guid.NewGuid();

        try
        {
            await _db
                .SaveChangesAsync(
                    ct);
        }
        catch (
            DbUpdateConcurrencyException)
        {
            return Conflict(
                "Ticket został zmieniony przez innego użytkownika.");
        }

        await _audit
            .WriteAsync(
                "UPDATE",
                "Ticket",
                id.ToString(),
                User.Identity?.Name
                ?? "unknown");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(
        Policy =
            "AdminOnly")]
    public async Task<IActionResult>
        Delete(
            int id,
            CancellationToken ct)
    {
        var ticket =
            await _db.Tickets
                .FirstOrDefaultAsync(
                    x =>
                        x.Id
                        == id,
                    ct);

        if (ticket is null)
        {
            return NotFound();
        }

        ticket.IsDeleted =
            true;

        ticket.DeletedAtUtc =
            DateTime.UtcNow;

        ticket.DeletedByUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        ticket.RowVersion =
            Guid.NewGuid();

        await _db
            .SaveChangesAsync(
                ct);

        await _audit
            .WriteAsync(
                "DELETE",
                "Ticket",
                id.ToString(),
                User.Identity?.Name
                ?? "unknown");

        return NoContent();
    }

    private static IQueryable<Ticket> ApplySort(
        IQueryable<Ticket> query,
        string? sortBy,
        bool desc)
    {
        return (sortBy, desc) switch
        {
            ("Number", false) => query.OrderBy(
                x => x.Number),
            ("Number", true) => query.OrderByDescending(
                x => x.Number),
            ("Title", false) => query.OrderBy(
                x => x.Title),
            ("Title", true) => query.OrderByDescending(
                x => x.Title),
            ("Status", false) => query.OrderBy(
                x => x.Status),
            ("Status", true) => query.OrderByDescending(
                x => x.Status),
            ("Priority", false) => query.OrderBy(
                x => x.Priority),
            ("Priority", true) => query.OrderByDescending(
                x => x.Priority),
            ("CreatedAtUtc", false) => query.OrderBy(
                x => x.CreatedAtUtc),
            _ => query.OrderByDescending(
                x => x.CreatedAtUtc)
        };
    }

    private static TicketDto ToDto(
        Ticket ticket)
    {
        return new TicketDto
        {
            Id =
                ticket.Id,

            Number =
                ticket.Number,

            Title =
                ticket.Title,

            Description =
                ticket.Description,

            Status =
                ticket.Status,

            Priority =
                ticket.Priority,

            CreatedAtUtc =
                ticket.CreatedAtUtc,

            UpdatedAtUtc =
                ticket.UpdatedAtUtc,

            RowVersionBase64 =
                EncodeRowVersion(
                    ticket.RowVersion)
        };
    }

    private static string EncodeRowVersion(
        Guid rowVersion)
    {
        return Convert.ToBase64String(
            rowVersion.ToByteArray());
    }

    private static bool TryDecodeRowVersion(
        string? value,
        out Guid rowVersion)
    {
        rowVersion =
            Guid.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            return false;
        }

        try
        {
            var bytes =
                Convert.FromBase64String(
                    value);

            if (bytes.Length != 16)
            {
                return false;
            }

            rowVersion =
                new Guid(
                    bytes);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

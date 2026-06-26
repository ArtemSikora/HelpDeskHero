using HelpDeskHero.Api.Infrastructure;
using HelpDeskHero.Shared.Contracts.Dashboard;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController
    : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<TicketDashboardDto>> Get(
        CancellationToken ct)
    {
        var visibleTickets =
            _db.Tickets
                .AsNoTracking();

        var openTickets =
            await visibleTickets
                .CountAsync(
                    x => x.Status != "Closed",
                    ct);

        var closedTickets =
            await visibleTickets
                .CountAsync(
                    x => x.Status == "Closed",
                    ct);

        var highPriorityOpenTickets =
            await visibleTickets
                .CountAsync(
                    x =>
                        x.Status != "Closed"
                        && x.Priority == "High",
                    ct);

        var deletedTickets =
            await _db.Tickets
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(
                    x => x.IsDeleted,
                    ct);

        var byStatus =
            await visibleTickets
                .GroupBy(
                    x => x.Status)
                .Select(
                    x => new
                    {
                        Name = x.Key,
                        Count = x.Count()
                    })
                .ToDictionaryAsync(
                    x => x.Name,
                    x => x.Count,
                    ct);

        var byPriority =
            await visibleTickets
                .GroupBy(
                    x => x.Priority)
                .Select(
                    x => new
                    {
                        Name = x.Key,
                        Count = x.Count()
                    })
                .ToDictionaryAsync(
                    x => x.Name,
                    x => x.Count,
                    ct);

        var recentTickets =
            await visibleTickets
                .OrderByDescending(
                    x => x.CreatedAtUtc)
                .Take(
                    5)
                .Select(
                    x => new RecentTicketDto
                    {
                        Id = x.Id,
                        Number = x.Number,
                        Title = x.Title,
                        Status = x.Status,
                        Priority = x.Priority,
                        CreatedAtUtc = x.CreatedAtUtc
                    })
                .ToListAsync(
                    ct);

        return Ok(
            new TicketDashboardDto
            {
                OpenTickets = openTickets,
                ClosedTickets = closedTickets,
                DeletedTickets = deletedTickets,
                HighPriorityOpenTickets = highPriorityOpenTickets,
                ByStatus = byStatus,
                ByPriority = byPriority,
                RecentTickets = recentTickets
            });
    }
}

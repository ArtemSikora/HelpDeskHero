using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure;
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

    public TicketsController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetAll()
    {
        var tickets =
            await _db.Tickets
                .OrderByDescending(
                    x => x.Id)
                .Select(
                    x => new TicketDto
                    {
                        Id = x.Id,
                        Number = x.Number,
                        Title = x.Title,
                        Description = x.Description,
                        Status = x.Status,
                        Priority = x.Priority,
                        CreatedAtUtc = x.CreatedAtUtc
                    })
                .ToListAsync();

        return Ok(
            tickets);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Create(
        CreateTicketDto dto)
    {
        var nextNumber =
            $"HDH-{(await _db.Tickets.CountAsync() + 1):0000}";

        var ticket =
            new Ticket
            {
                Number =
                    nextNumber,

                Title =
                    dto.Title,

                Description =
                    dto.Description,

                Priority =
                    dto.Priority,

                Status =
                    "New",

                CreatedAtUtc =
                    DateTime.UtcNow
            };

        _db.Tickets.Add(
            ticket);

        await _db.SaveChangesAsync();

        return Ok(
            new TicketDto
            {
                Id = ticket.Id,
                Number = ticket.Number,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAtUtc = ticket.CreatedAtUtc
            });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AgentOrAdmin")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTicketDto dto)
    {
        var ticket =
            await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        ticket.Title =
            dto.Title;

        ticket.Description =
            dto.Description;

        ticket.Priority =
            dto.Priority;

        ticket.Status =
            dto.Status;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var ticket =
            await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        _db.Tickets.Remove(
            ticket);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
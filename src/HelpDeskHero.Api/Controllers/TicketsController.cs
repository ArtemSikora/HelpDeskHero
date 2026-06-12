using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ControllerBase
{
    private static readonly List<TicketDto> Tickets =
    [
        new()
        {
            Id = 1,
            Number = "HDH-0001",
            Title = "Printer not working",
            Description = "Office printer shows paper jam.",
            Status = "New",
            Priority = "High",
            CreatedAtUtc = DateTime.UtcNow
        }
    ];

    [HttpGet]
    public ActionResult<IReadOnlyList<TicketDto>> GetAll()
    {
        return Ok(Tickets);
    }

    [HttpPost]
    public ActionResult<TicketDto> Create(CreateTicketDto dto)
    {
        var nextId =
            Tickets.Count == 0
                ? 1
                : Tickets.Max(x => x.Id) + 1;

        var ticket = new TicketDto
        {
            Id = nextId,
            Number = $"HDH-{nextId:0000}",
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow
        };

        Tickets.Add(ticket);

        return Ok(ticket);
    }

    [HttpPut("{id}")]
    public IActionResult Close(int id)
    {
        var ticket =
            Tickets.FirstOrDefault(
                x => x.Id == id);

        if (ticket is null)
            return NotFound();

        ticket.Status = "Closed";

        return NoContent();
    }
}
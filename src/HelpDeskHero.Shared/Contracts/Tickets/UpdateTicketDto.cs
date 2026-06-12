namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class UpdateTicketDto
{
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Status { get; set; } = "New";

    public string Priority { get; set; } = "Medium";
}
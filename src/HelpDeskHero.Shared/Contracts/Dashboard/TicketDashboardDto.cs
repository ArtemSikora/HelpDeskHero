namespace HelpDeskHero.Shared.Contracts.Dashboard;

public sealed class TicketDashboardDto
{
    public int OpenTickets { get; set; }

    public int ClosedTickets { get; set; }

    public int DeletedTickets { get; set; }

    public int HighPriorityOpenTickets { get; set; }

    public IReadOnlyDictionary<string, int> ByStatus { get; set; } =
        new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> ByPriority { get; set; } =
        new Dictionary<string, int>();

    public IReadOnlyList<RecentTicketDto> RecentTickets { get; set; } = [];
}

public sealed class RecentTicketDto
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}

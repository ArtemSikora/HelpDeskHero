using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Dashboard;

namespace HelpDeskHero.UI.Services.Api;

public sealed class DashboardApiClient
{
    private readonly HttpClient _httpClient;

    public DashboardApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TicketDashboardDto?> GetAsync()
    {
        return await _httpClient
            .GetFromJsonAsync<TicketDashboardDto>(
                "api/Dashboard");
    }
}

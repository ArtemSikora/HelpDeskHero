using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.UI.Services.Api;

public sealed class TicketApiClient
{
    private readonly HttpClient _httpClient;

    public TicketApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync()
    {
        var result =
            await _httpClient
                .GetFromJsonAsync<List<TicketDto>>(
                    "api/Tickets");

        return result ?? [];
    }

    public async Task CreateAsync(
        CreateTicketDto dto)
    {
        var response =
            await _httpClient
                .PostAsJsonAsync(
                    "api/Tickets",
                    dto);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(
        int id,
        UpdateTicketDto dto)
    {
        var response =
            await _httpClient
                .PutAsJsonAsync(
                    $"api/Tickets/{id}",
                    dto);

        response.EnsureSuccessStatusCode();
    }

    public async Task CloseAsync(
        int id)
    {
        var response =
            await _httpClient
                .PutAsync(
                    $"api/Tickets/{id}",
                    null);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        int id)
    {
        var response =
            await _httpClient
                .DeleteAsync(
                    $"api/Tickets/{id}");

        response.EnsureSuccessStatusCode();
    }
}
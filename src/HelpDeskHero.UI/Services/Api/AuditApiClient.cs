using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Audit;
using HelpDeskHero.Shared.Contracts.Common;

namespace HelpDeskHero.UI.Services.Api;

public sealed class AuditApiClient
{
    private readonly HttpClient _httpClient;

    public AuditApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync()
    {
        var result =
            await _httpClient
                .GetFromJsonAsync<PagedResultDto<AuditLogDto>>(
                    "api/Audit?pageSize=50");

        return result?.Items ?? [];
    }
}

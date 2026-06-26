using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Audit;
using HelpDeskHero.Shared.Contracts.Auth;
using HelpDeskHero.Shared.Contracts.Common;
using HelpDeskHero.Shared.Contracts.Dashboard;
using HelpDeskHero.Shared.Contracts.Tickets;
using Xunit;

namespace HelpDeskHero.Tests;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Auth_refresh_and_revoke_all_sessions_work()
    {
        using var factory =
            new HelpDeskHeroApiFactory();

        using var client =
            factory.CreateClient();

        var login =
            await LoginAsync(
                client);

        Assert.Equal(
            "admin",
            login.UserName);
        Assert.Equal(
            "Admin",
            login.Role);
        Assert.False(
            string.IsNullOrWhiteSpace(
                login.RefreshToken));

        client.DefaultRequestHeaders.Authorization =
            Bearer(
                login.AccessToken);

        var ticketsResponse =
            await client.GetAsync(
                "api/Tickets");

        Assert.Equal(
            HttpStatusCode.OK,
            ticketsResponse.StatusCode);

        var refreshResponse =
            await client.PostAsJsonAsync(
                "api/Auth/refresh",
                new RefreshTokenRequestDto
                {
                    RefreshToken =
                        login.RefreshToken,
                    DeviceName =
                        "IntegrationTest"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            refreshResponse.StatusCode);

        var refreshed =
            await refreshResponse.Content
                .ReadFromJsonAsync<RefreshTokenResponseDto>();

        Assert.NotNull(
            refreshed);

        var reusedOldRefresh =
            await client.PostAsJsonAsync(
                "api/Auth/refresh",
                new RefreshTokenRequestDto
                {
                    RefreshToken =
                        login.RefreshToken,
                    DeviceName =
                        "IntegrationTest"
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reusedOldRefresh.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            Bearer(
                refreshed!.AccessToken);

        var revokeAll =
            await client.PostAsync(
                "api/Auth/revoke-all",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            revokeAll.StatusCode);

        var revokeDto =
            await revokeAll.Content
                .ReadFromJsonAsync<RevokeAllSessionsResponseDto>();

        Assert.True(
            revokeDto!.RevokedCount > 0);

        var refreshAfterRevokeAll =
            await client.PostAsJsonAsync(
                "api/Auth/refresh",
                new RefreshTokenRequestDto
                {
                    RefreshToken =
                        refreshed.RefreshToken,
                    DeviceName =
                        "IntegrationTest"
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshAfterRevokeAll.StatusCode);
    }

    [Fact]
    public async Task Tickets_audit_dashboard_export_and_restore_work()
    {
        using var factory =
            new HelpDeskHeroApiFactory();

        using var client =
            factory.CreateClient();

        var login =
            await LoginAsync(
                client);

        client.DefaultRequestHeaders.Authorization =
            Bearer(
                login.AccessToken);

        var createdResponse =
            await client.PostAsJsonAsync(
                "api/Tickets",
                new CreateTicketDto
                {
                    Title =
                        "Integration restore/export ticket",
                    Description =
                        "Created from integration tests.",
                    Priority =
                        "High"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            createdResponse.StatusCode);

        var created =
            await createdResponse.Content
                .ReadFromJsonAsync<TicketDto>();

        Assert.NotNull(
            created);

        var deleteResponse =
            await client.DeleteAsync(
                $"api/Tickets/{created!.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var deletedPage =
            await client.GetFromJsonAsync<PagedResultDto<TicketDto>>(
                "api/Tickets?deletedOnly=true&pageSize=20");

        Assert.Contains(
            deletedPage!.Items,
            x =>
                x.Id == created.Id
                && x.IsDeleted);

        var restoreResponse =
            await client.PostAsync(
                $"api/Tickets/{created.Id}/restore",
                null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            restoreResponse.StatusCode);

        var dashboard =
            await client.GetFromJsonAsync<TicketDashboardDto>(
                "api/Dashboard");

        Assert.NotNull(
            dashboard);
        Assert.True(
            dashboard!.OpenTickets >= 1);
        Assert.True(
            dashboard.HighPriorityOpenTickets >= 1);

        var csv =
            await client.GetStringAsync(
                "api/Tickets/export?includeDeleted=true");

        Assert.Contains(
            "Integration restore/export ticket",
            csv);

        var audit =
            await client.GetFromJsonAsync<PagedResultDto<AuditLogDto>>(
                "api/Audit?pageSize=20");

        Assert.Contains(
            audit!.Items,
            x =>
                x.EntityName == "Ticket"
                && x.EntityId == created.Id.ToString()
                && x.Action == "RESTORE");
    }

    private static async Task<LoginResponseDto> LoginAsync(
        HttpClient client)
    {
        var response =
            await client.PostAsJsonAsync(
                "api/Auth/login",
                new LoginRequestDto
                {
                    UserName =
                        "admin",
                    Password =
                        "Admin123!",
                    DeviceName =
                        "IntegrationTest"
                });

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<LoginResponseDto>())!;
    }

    private static AuthenticationHeaderValue Bearer(
        string token)
    {
        return new AuthenticationHeaderValue(
            "Bearer",
            token);
    }
}

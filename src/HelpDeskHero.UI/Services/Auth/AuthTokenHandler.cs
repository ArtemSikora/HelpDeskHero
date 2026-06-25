using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using HelpDeskHero.Shared.Contracts.Auth;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class AuthTokenHandler
    : DelegatingHandler
{
    private readonly TokenStorageService
        _tokenStorage;

    public AuthTokenHandler(
        TokenStorageService tokenStorage)
    {
        _tokenStorage =
            tokenStorage;
    }

    protected override async Task<HttpResponseMessage>
        SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        var token =
            await _tokenStorage
                .GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(
                token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        var response =
            await base.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode
            != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshToken =
            await _tokenStorage
                .GetRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(
                refreshToken))
        {
            return response;
        }

        using var client =
            new HttpClient
            {
                BaseAddress =
                    new Uri(
                        "http://localhost:5067")
            };

        var refreshResponse =
            await client
                .PostAsJsonAsync(
                    "api/Auth/refresh",
                    new RefreshTokenRequestDto
                    {
                        RefreshToken =
                            refreshToken,

                        DeviceName =
                            "Blazor WebAssembly"
                    });

        if (!refreshResponse
                .IsSuccessStatusCode)
        {
            await _tokenStorage
                .RemoveTokenAsync();

            await _tokenStorage
                .RemoveRefreshTokenAsync();

            await _tokenStorage
                .RemoveRoleAsync();

            return response;
        }

        var dto =
            await refreshResponse
                .Content
                .ReadFromJsonAsync<
                    RefreshTokenResponseDto>();

        if (dto is null)
        {
            return response;
        }

        await _tokenStorage
            .SetTokenAsync(
                string.IsNullOrWhiteSpace(
                    dto.AccessToken)
                    ? dto.Token
                    : dto.AccessToken);

        await _tokenStorage
            .SetRefreshTokenAsync(
                dto.RefreshToken);

        var role =
            await _tokenStorage
                .GetRoleAsync();

        if (!string.IsNullOrWhiteSpace(
                role))
        {
            await _tokenStorage
                .SetRoleAsync(
                    role);
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                string.IsNullOrWhiteSpace(
                    dto.AccessToken)
                    ? dto.Token
                    : dto.AccessToken);

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}

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

        if (JwtTokenClock.ShouldRefresh(
                token,
                TimeSpan.FromMinutes(
                    1)))
        {
            token =
                await RefreshAccessTokenAsync(
                    cancellationToken)
                ?? token;
        }

        if (!string.IsNullOrWhiteSpace(
                token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        var retryRequest =
            await CloneRequestAsync(
                request,
                cancellationToken);

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

        var refreshedToken =
            await RefreshAccessTokenAsync(
                cancellationToken);

        if (string.IsNullOrWhiteSpace(
                refreshedToken))
        {
            return response;
        }

        retryRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                refreshedToken);

        return await base.SendAsync(
            retryRequest,
            cancellationToken);
    }

    private async Task<string?> RefreshAccessTokenAsync(
        CancellationToken cancellationToken)
    {
        var refreshToken =
            await _tokenStorage
                .GetRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(
                refreshToken))
        {
            return null;
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

            return null;
        }

        var dto =
            await refreshResponse
                .Content
                .ReadFromJsonAsync<
                    RefreshTokenResponseDto>();

        if (dto is null)
        {
            return null;
        }

        var accessToken =
            string.IsNullOrWhiteSpace(
                dto.AccessToken)
                ? dto.Token
                : dto.AccessToken;

        await _tokenStorage
            .SetTokenAsync(
                accessToken);

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

        return accessToken;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone =
            new HttpRequestMessage(
                request.Method,
                request.RequestUri)
            {
                Version =
                    request.Version,

                VersionPolicy =
                    request.VersionPolicy
            };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);
        }

        if (request.Content is not null)
        {
            var bytes =
                await request.Content
                    .ReadAsByteArrayAsync(
                        cancellationToken);

            clone.Content =
                new ByteArrayContent(
                    bytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value);
            }
        }

        return clone;
    }
}

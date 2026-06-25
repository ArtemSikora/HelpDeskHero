using System.Security.Claims;
using System.Net.Http.Json;

using HelpDeskHero.Shared.Contracts.Auth;

using Microsoft.AspNetCore.Components.Authorization;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class JwtAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly TokenStorageService
        _tokenStorage;

    public JwtAuthenticationStateProvider(
        TokenStorageService tokenStorage)
    {
        _tokenStorage =
            tokenStorage;
    }

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        var token =
            await _tokenStorage
                .GetTokenAsync();

        var role =
            await _tokenStorage
                .GetRoleAsync();

        if (string.IsNullOrWhiteSpace(
                token))
        {
            var refreshToken =
                await _tokenStorage
                    .GetRefreshTokenAsync();

            if (!string.IsNullOrWhiteSpace(
                    refreshToken))
            {
                using var client =
                    new HttpClient
                    {
                        BaseAddress =
                            new Uri(
                                "http://localhost:5067")
                    };

                var response =
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

                if (response.IsSuccessStatusCode)
                {
                    var dto =
                        await response
                            .Content
                            .ReadFromJsonAsync<
                                RefreshTokenResponseDto>();

                    if (dto is not null)
                    {
                        await _tokenStorage
                            .SetTokenAsync(
                                string.IsNullOrWhiteSpace(
                                    dto.AccessToken)
                                    ? dto.Token
                                    : dto.AccessToken);

                        await _tokenStorage
                            .SetRefreshTokenAsync(
                                dto.RefreshToken);

                        token =
                            string.IsNullOrWhiteSpace(
                                dto.AccessToken)
                                ? dto.Token
                                : dto.AccessToken;
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(
                token))
        {
            return Anonymous();
        }

        var claims =
            new List<Claim>();

        if (!string.IsNullOrWhiteSpace(
                role))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var identity =
            new ClaimsIdentity(
                claims,
                "jwt");

        return new AuthenticationState(
            new ClaimsPrincipal(
                identity));
    }

    public void NotifyLogin()
    {
        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public void NotifyLogout()
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(
                Anonymous()));
    }

    private static AuthenticationState
        Anonymous()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity()));
    }
}

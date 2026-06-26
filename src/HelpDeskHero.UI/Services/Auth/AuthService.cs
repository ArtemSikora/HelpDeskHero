using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Auth;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class AuthService
{
    private readonly HttpClient _http;

    private readonly TokenStorageService
        _tokenStorage;

    public AuthService(
        HttpClient http,
        TokenStorageService tokenStorage)
    {
        _http =
            http;

        _tokenStorage =
            tokenStorage;
    }

    public async Task<bool> LoginAsync(
        string userName,
        string password)
    {
        var request =
            new LoginRequestDto
            {
                UserName =
                    userName,

                Password =
                    password,

                DeviceName =
                    "Blazor WebAssembly"
            };

        var response =
            await _http
                .PostAsJsonAsync(
                    "api/Auth/login",
                    request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var dto =
            await response.Content
                .ReadFromJsonAsync<
                    LoginResponseDto>();

        if (dto is null)
        {
            return false;
        }

        await _tokenStorage
            .SetTokenAsync(
                string.IsNullOrWhiteSpace(
                    dto.AccessToken)
                    ? dto.Token
                    : dto.AccessToken);

        if (!string.IsNullOrWhiteSpace(
                dto.RefreshToken))
        {
            await _tokenStorage
                .SetRefreshTokenAsync(
                    dto.RefreshToken);
        }

        if (!string.IsNullOrWhiteSpace(
                dto.Role))
        {
            await _tokenStorage
                .SetRoleAsync(
                    dto.Role);
        }

        return true;
    }

    public async Task LogoutAsync()
    {
        var refreshToken =
            await _tokenStorage
                .GetRefreshTokenAsync();

        if (!string.IsNullOrWhiteSpace(
                refreshToken))
        {
            await _http
                .PostAsJsonAsync(
                    "api/Auth/revoke",
                    new RefreshTokenRequestDto
                    {
                        RefreshToken =
                            refreshToken,

                        DeviceName =
                            "Blazor WebAssembly"
                    });
        }

        await _tokenStorage
            .RemoveTokenAsync();

        await _tokenStorage
            .RemoveRefreshTokenAsync();

        await _tokenStorage
            .RemoveRoleAsync();
    }

    public async Task<int> RevokeAllSessionsAsync()
    {
        var response =
            await _http
                .PostAsync(
                    "api/Auth/revoke-all",
                    null);

        response.EnsureSuccessStatusCode();

        var dto =
            await response.Content
                .ReadFromJsonAsync<
                    RevokeAllSessionsResponseDto>();

        await LogoutAsync();

        return dto?.RevokedCount ?? 0;
    }
}

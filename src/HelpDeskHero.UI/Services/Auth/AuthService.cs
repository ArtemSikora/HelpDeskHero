using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Auth;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class AuthService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;

    public AuthService(
        HttpClient http,
        TokenStorageService tokenStorage)
    {
        _http = http;
        _tokenStorage = tokenStorage;
    }

    public async Task<bool> LoginAsync(
        string userName,
        string password)
    {
        var request =
            new LoginRequestDto
            {
                UserName = userName,
                Password = password
            };

        var response =
            await _http
                .PostAsJsonAsync(
                    "api/Auth/login",
                    request);

        if (!response.IsSuccessStatusCode)
            return false;

        var dto =
            await response.Content
                .ReadFromJsonAsync<LoginResponseDto>();

        if (dto is null)
            return false;

        await _tokenStorage
            .SetTokenAsync(
                dto.Token);

        return true;
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage
            .RemoveTokenAsync();
    }
}
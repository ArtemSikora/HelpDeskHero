using Microsoft.JSInterop;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class TokenStorageService
{
    private readonly IJSRuntime _js;

    public TokenStorageService(
        IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask<string?> GetTokenAsync()
    {
        return _js.InvokeAsync<string?>(
            "authStorage.getToken");
    }

    public ValueTask SetTokenAsync(
        string token)
    {
        return _js.InvokeVoidAsync(
            "authStorage.setToken",
            token);
    }

    public ValueTask RemoveTokenAsync()
    {
        return _js.InvokeVoidAsync(
            "authStorage.removeToken");
    }

    public ValueTask<string?> GetRefreshTokenAsync()
    {
        return _js.InvokeAsync<string?>(
            "authStorage.getRefreshToken");
    }

    public ValueTask SetRefreshTokenAsync(
        string token)
    {
        return _js.InvokeVoidAsync(
            "authStorage.setRefreshToken",
            token);
    }

    public ValueTask RemoveRefreshTokenAsync()
    {
        return _js.InvokeVoidAsync(
            "authStorage.removeRefreshToken");
    }

    public ValueTask<string?> GetRoleAsync()
    {
        return _js.InvokeAsync<string?>(
            "authStorage.getRole");
    }

    public ValueTask SetRoleAsync(
        string role)
    {
        return _js.InvokeVoidAsync(
            "authStorage.setRole",
            role);
    }

    public ValueTask RemoveRoleAsync()
    {
        return _js.InvokeVoidAsync(
            "authStorage.removeRole");
    }
}
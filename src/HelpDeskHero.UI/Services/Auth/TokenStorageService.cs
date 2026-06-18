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
}
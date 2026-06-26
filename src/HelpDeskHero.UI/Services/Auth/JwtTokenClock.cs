using System.Text.Json;

namespace HelpDeskHero.UI.Services.Auth;

public static class JwtTokenClock
{
    public static bool ShouldRefresh(
        string? token,
        TimeSpan threshold)
    {
        var expiresAtUtc =
            GetExpiresAtUtc(
                token);

        return expiresAtUtc is not null
            && expiresAtUtc.Value <= DateTimeOffset.UtcNow.Add(
                threshold);
    }

    private static DateTimeOffset? GetExpiresAtUtc(
        string? token)
    {
        if (string.IsNullOrWhiteSpace(
                token))
        {
            return null;
        }

        var parts =
            token.Split(
                '.');

        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            var payload =
                parts[1]
                    .Replace(
                        '-',
                        '+')
                    .Replace(
                        '_',
                        '/');

            payload =
                payload.PadRight(
                    payload.Length + (4 - payload.Length % 4) % 4,
                    '=');

            var json =
                JsonDocument.Parse(
                    Convert.FromBase64String(
                        payload));

            if (!json.RootElement.TryGetProperty(
                    "exp",
                    out var exp))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(
                exp.GetInt64());
        }
        catch
        {
            return null;
        }
    }
}

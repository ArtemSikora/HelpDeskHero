using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDeskHero.Api.Security;

public sealed class RefreshTokenService
{
    private readonly AppDbContext _db;

    private readonly ITokenService _tokenService;

    private readonly JwtOptions _jwt;

    public RefreshTokenService(
        AppDbContext db,
        ITokenService tokenService,
        IOptions<JwtOptions> jwt)
    {
        _db = db;
        _tokenService = tokenService;
        _jwt = jwt.Value;
    }

    public async Task<(string rawToken, DateTime expiresAtUtc)> CreateAsync(
        string userId,
        string deviceName,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var (rawToken, hash) =
            _tokenService.CreateRefreshToken();

        var expiresAtUtc =
            DateTime.UtcNow.AddDays(
                _jwt.RefreshTokenDays);

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                DeviceName = NormalizeDeviceName(
                    deviceName),
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = expiresAtUtc
            });

        await _db.SaveChangesAsync(
            ct);

        return (
            rawToken,
            expiresAtUtc);
    }

    public async Task<RefreshToken?> GetActiveByRawTokenAsync(
        string rawToken,
        CancellationToken ct = default)
    {
        var hash =
            _tokenService.ComputeRefreshTokenHash(
                rawToken);

        var token =
            await _db.RefreshTokens
                .Include(
                    x => x.User)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == hash,
                    ct);

        return token is { IsActive: true }
            ? token
            : null;
    }

    public async Task RevokeAsync(
        RefreshToken refreshToken,
        CancellationToken ct = default)
    {
        refreshToken.RevokedAtUtc =
            DateTime.UtcNow;

        await _db.SaveChangesAsync(
            ct);
    }

    public async Task<int> RevokeAllAsync(
        string userId,
        CancellationToken ct = default)
    {
        var activeTokens =
            await _db.RefreshTokens
                .Where(
                    x =>
                        x.UserId == userId
                        && x.RevokedAtUtc == null
                        && x.ExpiresAtUtc > DateTime.UtcNow)
                .ToListAsync(
                    ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc =
                DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(
            ct);

        return activeTokens.Count;
    }

    public static string NormalizeDeviceName(
        string? deviceName)
    {
        return string.IsNullOrWhiteSpace(
                deviceName)
            ? "Unknown"
            : deviceName.Trim();
    }
}

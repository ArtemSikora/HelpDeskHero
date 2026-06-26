using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HelpDeskHero.Tests;

public sealed class HelpDeskHeroApiFactory
    : WebApplicationFactory<Program>
{
    private readonly string _dbPath =
        Path.Combine(
            Path.GetTempPath(),
            $"helpdeskhero-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "Development");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            $"Data Source={_dbPath}",
                        ["Jwt:Issuer"] =
                            "HelpDeskHero.Api",
                        ["Jwt:Audience"] =
                            "HelpDeskHero.UI",
                        ["Jwt:Key"] =
                            "SUPER_SECRET_DEV_KEY_12345678901234567890",
                        ["Jwt:AccessTokenMinutes"] =
                            "15",
                        ["Jwt:RefreshTokenDays"] =
                            "7"
                    });
            });
    }

    protected override void Dispose(
        bool disposing)
    {
        base.Dispose(
            disposing);

        TryDelete(
            _dbPath);
        TryDelete(
            $"{_dbPath}-shm");
        TryDelete(
            $"{_dbPath}-wal");
    }

    private static void TryDelete(
        string path)
    {
        if (File.Exists(
                path))
        {
            try
            {
                File.Delete(
                    path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}

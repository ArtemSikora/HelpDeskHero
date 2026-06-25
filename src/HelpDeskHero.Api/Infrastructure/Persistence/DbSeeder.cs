using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        using var scope =
            services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<
                    AppDbContext>();

        await db.Database
            .MigrateAsync();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var roles =
            new[]
            {
                "Admin",
                "Manager",
                "Agent",
                "User"
            };

        foreach (var role in roles)
        {
            if (!await roleManager
                    .RoleExistsAsync(
                        role))
            {
                await roleManager
                    .CreateAsync(
                        new IdentityRole(
                            role));
            }
        }

        await CreateUser(
            userManager,
            "admin",
            "Admin123!",
            "Admin");

        await CreateUser(
            userManager,
            "agent",
            "Agent123!",
            "Agent");

        await CreateUser(
            userManager,
            "user",
            "User123!",
            "User");
    }

    private static async Task CreateUser(
        UserManager<ApplicationUser>
            userManager,

        string userName,

        string password,

        string role)
    {
        var existing =
            await userManager
                .FindByNameAsync(
                    userName);

        if (existing is not null)
        {
            return;
        }

        var user =
            new ApplicationUser
            {
                UserName =
                    userName,

                DisplayName =
                    userName,

                Role =
                    role,

                Email =
                    $"{userName}@hdh.local"
            };

        var result =
            await userManager
                .CreateAsync(
                    user,
                    password);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(
                    ", ",
                    result.Errors
                        .Select(
                            x => x.Description)));
        }

        await userManager
            .AddToRoleAsync(
                user,
                role);
    }
}

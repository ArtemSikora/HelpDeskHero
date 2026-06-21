using HelpDeskHero.Api.Domain;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Infrastructure;

public sealed class AppDbContext
    : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets =>
        Set<Ticket>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(
            modelBuilder);

        modelBuilder
            .Entity<Ticket>(
                x =>
                {
                    x.HasKey(
                        t => t.Id);

                    x.Property(
                        t => t.Number)
                        .HasMaxLength(
                            30)
                        .IsRequired();

                    x.Property(
                        t => t.Title)
                        .HasMaxLength(
                            200)
                        .IsRequired();

                    x.Property(
                        t => t.Description)
                        .HasMaxLength(
                            4000);

                    x.Property(
                        t => t.Priority)
                        .HasMaxLength(
                            30);

                    x.Property(
                        t => t.Status)
                        .HasMaxLength(
                            30);
                });

        modelBuilder
            .Entity<RefreshToken>(
                x =>
                {
                    x.HasKey(
                        r => r.Id);

                    x.Property(
                        r => r.TokenHash)
                        .HasMaxLength(
                            256)
                        .IsRequired();
                });
    }
}
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

    public DbSet<AuditLog> AuditLogs =>
        Set<AuditLog>();

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

                    x.HasQueryFilter(
                        t => !t.IsDeleted);

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
                            4000)
                        .IsRequired();

                    x.Property(
                        t => t.Priority)
                        .HasMaxLength(
                            30)
                        .IsRequired();

                    x.Property(
                        t => t.Status)
                        .HasMaxLength(
                            30)
                        .IsRequired();

                    x.Property(
                        t => t.RowVersion)
                        .IsConcurrencyToken();
                });

        modelBuilder
            .Entity<AuditLog>(
                x =>
                {
                    x.HasKey(
                        a => a.Id);

                    x.Property(
                        a => a.Action)
                        .HasMaxLength(
                            100)
                        .IsRequired();

                    x.Property(
                        a => a.EntityName)
                        .HasMaxLength(
                            100)
                        .IsRequired();

                    x.Property(
                        a => a.EntityId)
                        .HasMaxLength(
                            100)
                        .IsRequired();

                    x.Property(
                        a => a.UserName)
                        .HasMaxLength(
                            256);

                    x.Property(
                        a => a.IpAddress)
                        .HasMaxLength(
                            64);
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

                    x.Property(
                        r => r.DeviceName)
                        .HasMaxLength(
                            200)
                        .IsRequired();

                    x.Property(
                        r => r.IpAddress)
                        .HasMaxLength(
                            64);

                    x.HasOne(
                            r => r.User)
                        .WithMany()
                        .HasForeignKey(
                            r => r.UserId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
    }
}

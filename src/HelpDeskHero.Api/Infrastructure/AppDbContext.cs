using HelpDeskHero.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Infrastructure;

public sealed class AppDbContext
    : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets =>
        Set<Ticket>();

    public DbSet<AppUser> Users =>
        Set<AppUser>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Ticket>()
            .HasKey(
                x => x.Id);

        modelBuilder
            .Entity<Ticket>()
            .Property(
                x => x.Number)
            .HasMaxLength(
                20);

        modelBuilder
            .Entity<Ticket>()
            .Property(
                x => x.Title)
            .HasMaxLength(
                200);

        modelBuilder
            .Entity<Ticket>()
            .Property(
                x => x.Priority)
            .HasMaxLength(
                50);

        modelBuilder
            .Entity<AppUser>()
            .HasKey(
                x => x.Id);

        modelBuilder
            .Entity<AppUser>()
            .Property(
                x => x.UserName)
            .HasMaxLength(
                100);

        modelBuilder
            .Entity<AppUser>()
            .Property(
                x => x.PasswordHash)
            .HasMaxLength(
                500);

        modelBuilder
            .Entity<AppUser>()
            .Property(
                x => x.Role)
            .HasMaxLength(
                50);

        modelBuilder
            .Entity<RefreshToken>()
            .HasKey(
                x => x.Id);

        modelBuilder
            .Entity<RefreshToken>()
            .Property(
                x => x.Token)
            .HasMaxLength(
                500);

        modelBuilder
            .Entity<AppUser>()
            .HasMany(
                x => x.RefreshTokens)
            .WithOne(
                x => x.AppUser)
            .HasForeignKey(
                x => x.AppUserId);
    }
}
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
    }
}
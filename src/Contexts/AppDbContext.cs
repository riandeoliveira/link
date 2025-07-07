using JobScraperBot.Models;

using Microsoft.EntityFrameworkCore;

namespace JobScraperBot.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IgnoredJob> IgnoredJobs => Set<IgnoredJob>();

    static AppDbContext() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.ChatId)
            .IsUnique();

        modelBuilder.Entity<IgnoredJob>()
            .HasKey(x => new { x.UserId, x.JobId });

        base.OnModelCreating(modelBuilder);
    }
}

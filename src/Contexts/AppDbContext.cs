using LinkJoBot.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkJoBot.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IgnoredJob> IgnoredJobs => Set<IgnoredJob>();

    static AppDbContext() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}

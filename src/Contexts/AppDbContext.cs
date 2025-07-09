using LinkJobber.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkJobber.Contexts;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IgnoredJob> IgnoredJobs => Set<IgnoredJob>();
}

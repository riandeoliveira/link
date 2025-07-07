using JobScraperBot.Contexts;
using JobScraperBot.Interfaces;

namespace JobScraperBot.SeedWork;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}

using LinkJoBot.Contexts;
using LinkJoBot.Interfaces;

namespace LinkJoBot.SeedWork;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

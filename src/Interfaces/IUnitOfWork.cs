namespace JobScraperBot.Interfaces;

public interface IUnitOfWork
{
    public Task CommitAsync(CancellationToken cancellationToken);
}

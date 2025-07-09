namespace LinkJobber.Interfaces;

public interface IUnitOfWork
{
    public Task CommitAsync(CancellationToken cancellationToken);
}

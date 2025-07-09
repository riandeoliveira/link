using LinkJobber.Entities;

namespace LinkJobber.Interfaces;

public interface IJobSearchService
{
    public Task RunJobSearchAsync(User user, CancellationToken cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken);
}

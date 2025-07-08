using LinkJoBot.Entities;

namespace LinkJoBot.Interfaces;

public interface IJobSearchService
{
    public Task RunJobSearchAsync(User user, CancellationToken cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken);
}

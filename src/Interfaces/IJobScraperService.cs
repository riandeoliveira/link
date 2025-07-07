using JobScraperBot.Models;

namespace JobScraperBot.Interfaces;

public interface IJobScraperService
{
    public Task RunJobSearchAsync(User user, CancellationToken cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken);
}

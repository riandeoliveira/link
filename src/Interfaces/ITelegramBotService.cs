namespace JobScraperBot.Interfaces;

public interface ITelegramBotService
{
    public Task StartAsync(CancellationToken cancellationToken);
}

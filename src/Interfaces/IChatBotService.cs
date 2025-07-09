namespace LinkJobber.Interfaces;

public interface IChatBotService
{
    public Task StartAsync(CancellationToken cancellationToken);
}

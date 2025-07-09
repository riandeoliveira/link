namespace LinkJobber.Interfaces;

public interface IChatBotHandlerService
{
    public Task StartAsync(CancellationToken cancellationToken);
}

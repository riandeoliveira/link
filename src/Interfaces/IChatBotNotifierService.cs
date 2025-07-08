using LinkJoBot.Records;

namespace LinkJoBot.Interfaces;

public interface IChatBotNotifierService
{
    public Task SendAvailableCommandsMessageAsync(string chatId);

    public Task SendErrorMessageAsync(string chatId, string message);

    public Task SendJobFoundMessageAsync(string chatId, JobFoundMessageData data);

    public Task SendTextMessageAsync(string chatId, string message);
}

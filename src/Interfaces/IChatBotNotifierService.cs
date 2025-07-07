using JobScraperBot.Records;

namespace JobScraperBot.Interfaces;

public interface IChatBotNotifierService
{
    public Task SendErrorMessageAsync(string chatId, params string[] lines);

    public Task SendJobFoundMessageAsync(string chatId, JobFoundMessageData data);

    public Task SendMultilineMessageAsync(string chatId, params string[] lines);
}

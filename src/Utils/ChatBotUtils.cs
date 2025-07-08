using LinkJoBot.Constants;
using Telegram.Bot.Types;

namespace LinkJoBot.Utils;

public static class ChatBotUtils
{
    public static string GetCommandLabel(string commandName)
    {
        BotCommand? command = ChatBot.Commands.FirstOrDefault(x =>
            x.Command.Equals(commandName, StringComparison.OrdinalIgnoreCase)
        );

        return command is null
            ? throw new InvalidOperationException($"Command '/{commandName}' not found")
            : $"/{command.Command} – {command.Description}";
    }
}

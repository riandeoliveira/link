using LinkJobber.Constants;
using Telegram.Bot.Types;

namespace LinkJobber.Utils;

public static class ChatBotUtils
{
    public static string GetCommandLabel(string commandName)
    {
        var command = ChatBotCommands.All.FirstOrDefault(x =>
            x.Command.Equals(commandName, StringComparison.OrdinalIgnoreCase)
        );

        return command is null
            ? throw new InvalidOperationException($"Command '/{commandName}' not found")
            : $"/{command.Command} – {command.Description}";
    }
}

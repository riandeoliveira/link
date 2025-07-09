using LinkJobber.Constants;

namespace LinkJobber.Utils;

public static class ChatBotUtils
{
    public static string GetCommandLabel(string commandName)
    {
        var command = ChatBotCommands.All.FirstOrDefault(x =>
            x.Command.Equals(commandName, StringComparison.OrdinalIgnoreCase)
        );

        return command is not null
            ? $"/{command.Command} – {command.Description}"
            : throw new InvalidOperationException($"Command '/{commandName}' not found");
    }
}

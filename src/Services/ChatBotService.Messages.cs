using Telegram.Bot.Types;

namespace LinkJoBot.Services;

public partial class ChatBotService
{
    public static string GetCommandLabel(string commandName)
    {
        BotCommand? command = GetChatBotCommands()
            .FirstOrDefault(x => x.Command.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        return command is null
            ? throw new InvalidOperationException($"Command '/{commandName}' not found")
            : $"/{command.Command} – {command.Description}";
    }

    public async Task SendAvailableCommandsMessageAsync(string chatId)
    {
        await _chatBot.SendTextMessageAsync(
            chatId,
            $"""
            <b>📌 Comandos disponíveis:</b>

            {GetCommandLabel("help")}

            {GetCommandLabel("ignore")}

            {GetCommandLabel("keywords")}

            {GetCommandLabel("limit")}

            {GetCommandLabel("postedtime")}

            {GetCommandLabel("reset")}

            {GetCommandLabel("search")}

            {GetCommandLabel("status")}

            {GetCommandLabel("worktype")}
            """
        );
    }
}

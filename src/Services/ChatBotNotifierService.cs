using System.Globalization;

using JobScraperBot.Interfaces;
using JobScraperBot.Records;

using Telegram.Bot;

namespace JobScraperBot.Services;

public class ChatBotNotifierService(ITelegramBotClient botClient) : IChatBotNotifierService
{
    private readonly ITelegramBotClient _botClient = botClient;

    public async Task SendErrorMessageAsync(string chatId, params string[] lines)
    {
        string message = string.Join("\n", lines);

        DateTime currentDate = DateTime.Now;

        string date = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        string time = currentDate.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        await SendMultilineMessageAsync(
            chatId,
            "<b>❌ ERRO ❌</b>",
            "",
            $"<b>📅 Data:</b> {date}",
            "",
            $"<b>🕒 Hora:</b> {time}",
            "",
            message
        );
    }

    public async Task SendJobFoundMessageAsync(string chatId, JobFoundMessageData data)
    {
        string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        string message = $"""
        <b>🚨 NOVA VAGA ENCONTRADA - {data.JobIndex}/{data.TotalJobs} 🚨</b>

        <b>🕒 {currentDate}</b>

        <b>🔠 Título:</b> {data.Title}

        <b>🏢 Empresa:</b> {data.Company}

        <b>📍 Região:</b> {data.Region}

        <b>🔵 Simplificada:</b> {data.EasyApply}

        <b>📅 Postagem:</b> {data.PostedTime}

        <a href="{data.Link}"><b>🔗 Acesse aqui!</b></a>
        """;

        await SendMultilineMessageAsync(chatId, message);
    }

    public async Task SendMultilineMessageAsync(string chatId, params string[] lines)
    {
        string message = string.Join("\n", lines);

        await _botClient.SendMessage(chatId, message, Telegram.Bot.Types.Enums.ParseMode.Html);
    }
}

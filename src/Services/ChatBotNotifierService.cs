using System.Globalization;
using LinkJobber.Interfaces;
using LinkJobber.Records;
using LinkJobber.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace LinkJobber.Services;

public class ChatBotNotifierService(ITelegramBotClient botClient) : IChatBotNotifierService
{
    private readonly ITelegramBotClient _botClient = botClient;

    public async Task SendAvailableCommandsMessageAsync(string chatId)
    {
        await SendTextMessageAsync(
            chatId,
            $"""
            <b>📌 Comandos disponíveis:</b>

            {ChatBotUtils.GetCommandLabel("help")}

            {ChatBotUtils.GetCommandLabel("ignore")}

            {ChatBotUtils.GetCommandLabel("keywords")}

            {ChatBotUtils.GetCommandLabel("limit")}

            {ChatBotUtils.GetCommandLabel("postedtime")}

            {ChatBotUtils.GetCommandLabel("reset")}

            {ChatBotUtils.GetCommandLabel("search")}

            {ChatBotUtils.GetCommandLabel("status")}

            {ChatBotUtils.GetCommandLabel("worktype")}
            """
        );
    }

    public async Task SendErrorMessageAsync(string chatId, string message)
    {
        var currentDate = DateTime.Now;

        var date = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var time = currentDate.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        await SendTextMessageAsync(
            chatId,
            $"""
            <b>❌ ERRO ❌</b>

            <b>📅 Data:</b> {date}

            <b>🕒 Hora:</b> {time}

            {message}
            """
        );
    }

    public async Task SendJobFoundMessageAsync(string chatId, JobFoundMessageData data)
    {
        var currentDate = DateTime.Now.ToString(
            "dd/MM/yyyy HH:mm:ss",
            CultureInfo.InvariantCulture
        );

        await SendTextMessageAsync(
            chatId,
            $"""
            <b>🚨 NOVA VAGA ENCONTRADA - {data.JobIndex}/{data.TotalJobs} 🚨</b>

            <b>🕒 {currentDate}</b>

            <b>🔠 Título:</b> {data.Title}

            <b>🏢 Empresa:</b> {data.Company}

            <b>📍 Região:</b> {data.Region}

            <b>🔵 Simplificada:</b> {data.HasEasyApply}

            <b>📅 Postagem:</b> {data.PostedTime}

            <a href="{data.Link}"><b>🔗 Acesse aqui!</b></a>
            """
        );
    }

    public async Task SendTextMessageAsync(string chatId, string message)
    {
        await _botClient.SendMessage(chatId, message, ParseMode.Html);
    }
}

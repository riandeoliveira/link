using LinkJoBot.Entities;
using LinkJoBot.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace LinkJoBot.Services;

public partial class ChatBotService
{
    public async Task HandleHelpCommandAsync(string chatId)
    {
        await SendAvailableCommandsMessageAsync(chatId);
    }

    public async Task HandleIgnoreCommandAsync(string chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData("Sim", "ignore_true"),
                    InlineKeyboardButton.WithCallbackData("Não", "ignore_false"),
                ],
            ]
        );

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Você deseja ignorar as vagas já encontradas daqui pra frente?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleIgnoreCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        bool ignoreJobsFound = bool.Parse(response);

        user.IgnoreJobsFound = ignoreJobsFound;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (ignoreJobsFound)
        {
            await _chatBot.SendTextMessageAsync(
                user.ChatId,
                "✅ Vagas já encontradas não aparecerão novamente a partir de agora!"
            );
        }
        else
        {
            await _chatBot.SendTextMessageAsync(
                user.ChatId,
                "✅ Você receberá vagas que já foram encontradas anteriormente!"
            );
        }
    }

    public async Task HandleKeywordsCommandAsync(User user, CancellationToken cancellationToken)
    {
        user.IsAwaitingForKeywords = true;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            "Digite as palavras-chaves que serão usadas na busca:"
        );
    }

    public async Task HandleKeywordsCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        string keywords = response.Trim();

        user.Keywords = keywords;
        user.IsAwaitingForKeywords = false;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Palavras-chave salvas como:

            <b>{keywords}</b>
            """
        );
    }

    public async Task HandleLimitCommandAsync(string chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData("5", "limit_5"),
                    InlineKeyboardButton.WithCallbackData("10", "limit_10"),
                ],
                [
                    InlineKeyboardButton.WithCallbackData("15", "limit_15"),
                    InlineKeyboardButton.WithCallbackData("20", "limit_20"),
                ],
            ]
        );

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Selecione o total de vagas para buscar:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleLimitCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        int limit = int.Parse(response);

        user.Limit = limit;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Limite salvo para buscar até:

            <b>{limit} vagas</b>
            """
        );
    }

    public async Task HandlePostedTimeCommandAsync(
        string chatId,
        CancellationToken cancellationToken
    )
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(3600),
                        "postedtime_3600"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(14400),
                        "postedtime_14400"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(28800),
                        "postedtime_28800"
                    ),
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(43200),
                        "postedtime_43200"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(86400),
                        "postedtime_86400"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetPostedTimeLabel(null),
                        "postedtime_null"
                    ),
                ],
            ]
        );

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Selecione o tempo máximo desde a publicação das vagas:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandlePostedTimeCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        int? postedTime = int.TryParse(response, out int parsed) ? parsed : null;

        user.PostedTime = postedTime;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        string label = GetPostedTimeLabel(postedTime);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Tempo máximo desde a publicação das vagas definido como:

            <b>{label}</b>
            """
        );
    }

    public async Task HandleResetCommandAsync(string chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData("Sim", "reset_true"),
                    InlineKeyboardButton.WithCallbackData("Não", "reset_false"),
                ],
            ]
        );

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Selecione se você deseja redefinir todos os parâmetros já configurados:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleResetCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        bool reset = bool.Parse(response);

        if (!reset)
        {
            await _chatBot.SendTextMessageAsync(user.ChatId, "Nenhum parâmetro redefinido.");

            return;
        }

        user.IgnoreJobsFound = false;
        user.WorkType = WorkType.All;
        user.Limit = 5;
        user.PostedTime = null;
        user.Keywords = string.Empty;

        await _ignoredJobRepository.DeleteManyAsync(x => x.UserId == user.Id, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            """
            ✅ Todos os parâmetros redefinidos com sucesso!

            Use /status para conferir!
            """
        );
    }

    public async Task HandleSearchCommandAsync(User user, CancellationToken cancellationToken)
    {
        await _jobSearchService.RunJobSearchAsync(user, cancellationToken);
    }

    public async Task HandleStartCommandAsync(string chatId)
    {
        await _chatBot.SendTextMessageAsync(
            chatId,
            """
            <b>👋 Olá! Eu sou o LinkJoBot 👋</b>

            Vou te ajudar a encontrar vagas no LinkedIn de forma automática, com base nas suas preferências!
            """
        );

        await SendAvailableCommandsMessageAsync(chatId);
    }

    public async Task HandleStatusCommandAsync(User user, CancellationToken cancellationToken)
    {
        string workTypeLabel = GetWorkTypeLabel(user.WorkType);
        string postedTimeLabel = GetPostedTimeLabel(user.PostedTime);

        int totalIgnoredJobs = await _ignoredJobRepository.CountAsync(
            x => x.UserId == user.Id,
            cancellationToken
        );

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            Seus parâmetros configurados atualmente:

            <b>🏠 Modelo de trabalho:</b>

            {workTypeLabel}

            <b>💼 Vagas para buscar:</b>

            {user.Limit}

            <b>📅 Publicação de até:</b>

            {postedTimeLabel}

            <b>🔠 Palavras-chave:</b>

            {user.Keywords}

            <b>⛔ Ignorar vagas já encontradas:</b>

            {(user.IgnoreJobsFound ? "Sim" : "Não")}

            <b>🚫 Total de vagas já ignoradas:</b>

            {totalIgnoredJobs}
            """
        );
    }

    public async Task HandleWorkTypeCommandAsync(string chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        GetWorkTypeLabel(WorkType.OnSite),
                        "worktype_OnSite"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetWorkTypeLabel(WorkType.Remote),
                        "worktype_Remote"
                    ),
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        GetWorkTypeLabel(WorkType.Hybrid),
                        "worktype_Hybrid"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        GetWorkTypeLabel(WorkType.All),
                        "worktype_All"
                    ),
                ],
            ]
        );

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Selecione o modelo de trabalho desejado:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleWorkTypeCommandResponseAsync(
        User user,
        string response,
        CancellationToken cancellationToken
    )
    {
        _ = Enum.TryParse(response, out WorkType workType);

        user.WorkType = workType;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        string label = GetWorkTypeLabel(workType);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Modelo de trabalho salvo como:

            <b>{label}</b>
            """
        );
    }
}

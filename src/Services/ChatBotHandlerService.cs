using LinkJobber.Constants;
using LinkJobber.Enums;
using LinkJobber.Interfaces;
using LinkJobber.Utils;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LinkJobber.Services;

public class ChatBotHandlerService(
    IChatBotNotifierService chatBot,
    IIgnoredJobRepository ignoredJobRepository,
    IJobSearchService jobSearchService,
    ITelegramBotClient botClient,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository
) : IChatBotHandlerService
{
    private readonly IChatBotNotifierService _chatBot = chatBot;
    private readonly IIgnoredJobRepository _ignoredJobRepository = ignoredJobRepository;
    private readonly IJobSearchService _jobSearchService = jobSearchService;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;

    private static readonly Dictionary<string, Entities.User> _userCache = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _botClient.SetMyCommands(ChatBotCommands.All, cancellationToken: cancellationToken);

        var options = new ReceiverOptions { DropPendingUpdates = true };

        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, options, cancellationToken);
    }

    private async Task<Entities.User> GetOrCreateUserByChatIdAsync(
        string chatId,
        CancellationToken cancellationToken
    )
    {
        if (_userCache.TryGetValue(chatId, out var cachedUser))
        {
            return cachedUser;
        }

        var user = await _userRepository.FindOneAsync(x => x.ChatId == chatId, cancellationToken);

        if (user is null)
        {
            user = new() { ChatId = chatId };

            await _userRepository.CreateAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        _userCache[chatId] = user;

        return user;
    }

    private async Task HandleCallbackQueryAsync(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        var chatId = callbackQuery.Message?.Chat.Id.ToString() ?? "";
        var data = callbackQuery.Data ?? "";

        var user = await GetOrCreateUserByChatIdAsync(chatId, cancellationToken);

        var prefixHandlers = new Dictionary<string, Func<string, Task>>()
        {
            ["ignore_"] = value => HandleIgnoreCommandResponseAsync(user, value, cancellationToken),
            ["limit_"] = value => HandleLimitCommandResponseAsync(user, value, cancellationToken),
            ["postedtime_"] = value =>
                HandlePostedTimeCommandResponseAsync(user, value, cancellationToken),
            ["reset_"] = value => HandleResetCommandResponseAsync(user, value, cancellationToken),
            ["worktype_"] = value =>
                HandleWorkTypeCommandResponseAsync(user, value, cancellationToken),
        };

        foreach ((string prefix, Func<string, Task> handler) in prefixHandlers)
        {
            if (data.StartsWith(prefix))
            {
                var value = data[prefix.Length..];

                await handler(value);

                break;
            }
        }

        await _botClient.AnswerCallbackQuery(
            callbackQuery.Id,
            cancellationToken: cancellationToken
        );
    }

    private Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    private async Task HandleHelpCommandRequestAsync(string chatId)
    {
        await _chatBot.SendAvailableCommandsMessageAsync(chatId);
    }

    private async Task HandleIgnoreCommandRequestAsync(
        string chatId,
        CancellationToken cancellationToken
    )
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

    private async Task HandleIgnoreCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        var ignoreJobsFound = bool.Parse(value);

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

    private async Task HandleKeywordsCommandRequestAsync(
        Entities.User user,
        CancellationToken cancellationToken
    )
    {
        user.IsAwaitingForKeywords = true;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            "Digite as palavras-chaves que serão usadas na busca:"
        );
    }

    private async Task HandleKeywordsCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        var keywords = value.Trim();

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

    private async Task HandleLimitCommandRequestAsync(
        string chatId,
        CancellationToken cancellationToken
    )
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

    private async Task HandleLimitCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        var limit = int.Parse(value);

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

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id.ToString();
        var text = message.Text!;

        var user = await GetOrCreateUserByChatIdAsync(chatId, cancellationToken);

        if (user.IsAwaitingForKeywords)
        {
            await HandleKeywordsCommandResponseAsync(user, text, cancellationToken);

            return;
        }

        Dictionary<string, Func<Task>> commandHandlers = new()
        {
            ["/help"] = () => HandleHelpCommandRequestAsync(chatId),
            ["/ignore"] = () => HandleIgnoreCommandRequestAsync(chatId, cancellationToken),
            ["/keywords"] = () => HandleKeywordsCommandRequestAsync(user, cancellationToken),
            ["/limit"] = () => HandleLimitCommandRequestAsync(chatId, cancellationToken),
            ["/postedtime"] = () => HandlePostedTimeCommandRequestAsync(chatId, cancellationToken),
            ["/reset"] = () => HandleResetCommandRequestAsync(chatId, cancellationToken),
            ["/search"] = () => HandleSearchCommandRequestAsync(user, cancellationToken),
            ["/start"] = () => HandleStartCommandRequestAsync(chatId),
            ["/status"] = () => HandleStatusCommandRequestAsync(user, cancellationToken),
            ["/worktype"] = () => HandleWorkTypeCommandRequestAsync(chatId, cancellationToken),
        };

        if (commandHandlers.TryGetValue(text, out var handler))
        {
            await handler();
        }
    }

    private async Task HandlePostedTimeCommandRequestAsync(
        string chatId,
        CancellationToken cancellationToken
    )
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(3600),
                        "postedtime_3600"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(14400),
                        "postedtime_14400"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(28800),
                        "postedtime_28800"
                    ),
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(43200),
                        "postedtime_43200"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(86400),
                        "postedtime_86400"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetPostedTimeLabel(null),
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

    private async Task HandlePostedTimeCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        int? postedTime = int.TryParse(value, out var parsed) ? parsed : null;

        user.PostedTime = postedTime;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        var label = JobUtils.GetPostedTimeLabel(postedTime);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Tempo máximo desde a publicação das vagas definido como:

            <b>{label}</b>
            """
        );
    }

    private async Task HandleResetCommandRequestAsync(
        string chatId,
        CancellationToken cancellationToken
    )
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

    private async Task HandleResetCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        var reset = bool.Parse(value);

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

    // TODO:

    private async Task HandleSearchCommandRequestAsync(
        Entities.User user,
        CancellationToken cancellationToken
    )
    {
        await _jobSearchService.RunJobSearchAsync(user, cancellationToken);
    }

    private async Task HandleStartCommandRequestAsync(string chatId)
    {
        await _chatBot.SendTextMessageAsync(
            chatId,
            """
            <b>👋 Olá! Eu sou o LinkJobber 👋</b>

            Vou te ajudar a encontrar vagas no LinkedIn de forma automática, com base nas suas preferências!
            """
        );

        await _chatBot.SendAvailableCommandsMessageAsync(chatId);
    }

    private async Task HandleStatusCommandRequestAsync(
        Entities.User user,
        CancellationToken cancellationToken
    )
    {
        var workTypeLabel = JobUtils.GetWorkTypeLabel(user.WorkType);
        var postedTimeLabel = JobUtils.GetPostedTimeLabel(user.PostedTime);

        var totalIgnoredJobs = await _ignoredJobRepository.CountAsync(
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

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        if (update.Message is { Text: { } })
        {
            await HandleMessageAsync(update.Message, cancellationToken);
        }
        else if (update.CallbackQuery?.Data is not null)
        {
            await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
        }
    }

    private async Task HandleWorkTypeCommandRequestAsync(
        string chatId,
        CancellationToken cancellationToken
    )
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetWorkTypeLabel(WorkType.OnSite),
                        "worktype_OnSite"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetWorkTypeLabel(WorkType.Remote),
                        "worktype_Remote"
                    ),
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetWorkTypeLabel(WorkType.Hybrid),
                        "worktype_Hybrid"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        JobUtils.GetWorkTypeLabel(WorkType.All),
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

    private async Task HandleWorkTypeCommandResponseAsync(
        Entities.User user,
        string value,
        CancellationToken cancellationToken
    )
    {
        _ = Enum.TryParse(value, out WorkType workType);

        user.WorkType = workType;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        var label = JobUtils.GetWorkTypeLabel(workType);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            ✅ Modelo de trabalho salvo como:

            <b>{label}</b>
            """
        );
    }
}

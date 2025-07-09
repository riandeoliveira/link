using LinkJobber.Constants;
using LinkJobber.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace LinkJobber.Services;

public partial class ChatBotService(
    IChatBotNotifierService chatBot,
    IIgnoredJobRepository ignoredJobRepository,
    IJobSearchService jobSearchService,
    ILogger<ChatBotService> logger,
    ITelegramBotClient botClient,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository
) : IChatBotService
{
    private readonly Dictionary<string, Entities.User> _userCache = [];
    private readonly IChatBotNotifierService _chatBot = chatBot;
    private readonly IIgnoredJobRepository _ignoredJobRepository = ignoredJobRepository;
    private readonly IJobSearchService _jobSearchService = jobSearchService;
    private readonly ILogger<IChatBotService> _logger = logger;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _botClient.SetMyCommands(ChatBot.Commands, cancellationToken: cancellationToken);

        ReceiverOptions options = new() { DropPendingUpdates = true };

        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, options, cancellationToken);

        User? me = await _botClient.GetMe(cancellationToken);

        _logger.Log(LogLevel.Information, "INICIANDO BOT");

        Console.WriteLine($"Bot iniciado. Username: @{me.Username}");
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

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        string chatId = message.Chat.Id.ToString();
        string text = message.Text!;

        Entities.User user = await GetOrCreateUserByChatIdAsync(chatId, cancellationToken);

        if (user.IsAwaitingForKeywords)
        {
            await HandleKeywordsCommandResponseAsync(user, text, cancellationToken);

            return;
        }

        Dictionary<string, Func<Task>> commandHandlers = new()
        {
            ["/help"] = () => HandleHelpCommandAsync(chatId),
            ["/ignore"] = () => HandleIgnoreCommandAsync(chatId, cancellationToken),
            ["/keywords"] = () => HandleKeywordsCommandAsync(user, cancellationToken),
            ["/limit"] = () => HandleLimitCommandAsync(chatId, cancellationToken),
            ["/postedtime"] = () => HandlePostedTimeCommandAsync(chatId, cancellationToken),
            ["/reset"] = () => HandleResetCommandAsync(chatId, cancellationToken),
            ["/search"] = () => HandleSearchCommandAsync(user, cancellationToken),
            ["/start"] = () => HandleStartCommandAsync(chatId),
            ["/status"] = () => HandleStatusCommandAsync(user, cancellationToken),
            ["/worktype"] = () => HandleWorkTypeCommandAsync(chatId, cancellationToken),
        };

        if (commandHandlers.TryGetValue(text, out Func<Task>? handler))
        {
            await handler();
        }
    }

    private async Task HandleCallbackQueryAsync(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        string chatId = callbackQuery.Message?.Chat.Id.ToString() ?? "";
        string data = callbackQuery.Data ?? "";

        Entities.User user = await GetOrCreateUserByChatIdAsync(chatId, cancellationToken);

        Dictionary<string, Func<string, Task>> prefixHandlers = new()
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
                string value = data[prefix.Length..];

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
        Console.WriteLine($"Erro no bot: {exception.Message}");

        return Task.CompletedTask;
    }

    public async Task<Entities.User> GetOrCreateUserByChatIdAsync(
        string chatId,
        CancellationToken cancellationToken
    )
    {
        if (_userCache.TryGetValue(chatId, out Entities.User? cachedUser))
            return cachedUser;

        Entities.User? user = await _userRepository.FindOneAsync(
            x => x.ChatId == chatId,
            cancellationToken
        );

        if (user is null)
        {
            user = new() { ChatId = chatId };

            await _userRepository.CreateAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        _userCache[chatId] = user;

        return user;
    }
}

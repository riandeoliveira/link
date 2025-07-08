using LinkJoBot.Enums;
using Telegram.Bot.Types;

namespace LinkJoBot.Services;

public partial class ChatBotService
{
    public static IEnumerable<BotCommand> GetChatBotCommands()
    {
        IEnumerable<BotCommand> commands =
        [
            new BotCommand
            {
                Command = "help",
                Description = "📖 Exibir todos os comandos disponíveis",
            },
            new BotCommand
            {
                Command = "ignore",
                Description = "🔁 Ativar/desativar exibição de vagas já enviadas anteriormente",
            },
            new BotCommand
            {
                Command = "keywords",
                Description = "📝 Definir palavras-chave para a busca",
            },
            new BotCommand
            {
                Command = "limit",
                Description = "🎯 Definir o número máximo de vagas por busca",
            },
            new BotCommand
            {
                Command = "postedtime",
                Description = "⏱️ Definir o tempo máximo desde a publicação da vaga",
            },
            new BotCommand
            {
                Command = "reset",
                Description = "♻️ Redefinir todas as configurações atuais",
            },
            new BotCommand
            {
                Command = "search",
                Description = "🔎 Iniciar a busca de vagas no LinkedIn",
            },
            new BotCommand { Command = "status", Description = "📋 Ver as configurações atuais" },
            new BotCommand
            {
                Command = "worktype",
                Description =
                    "🏢 Definir o modelo de trabalho (Presencial, Remoto, Híbrido ou Todos)",
            },
        ];

        return commands;
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

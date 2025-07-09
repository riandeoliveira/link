using Telegram.Bot.Types;

namespace LinkJobber.Constants;

public static class ChatBotCommands
{
    public static readonly IReadOnlyList<BotCommand> All =
    [
        new() { Command = "help", Description = "📖 Exibir todos os comandos disponíveis" },
        new()
        {
            Command = "ignore",
            Description = "🔁 Ativar/desativar exibição de vagas já enviadas anteriormente",
        },
        new() { Command = "keywords", Description = "📝 Definir palavras-chave para a busca" },
        new() { Command = "limit", Description = "🎯 Definir o número máximo de vagas por busca" },
        new()
        {
            Command = "postedtime",
            Description = "⏱️ Definir o tempo máximo desde a publicação da vaga",
        },
        new() { Command = "reset", Description = "♻️ Redefinir todas as configurações atuais" },
        new() { Command = "search", Description = "🔎 Iniciar a busca de vagas no LinkedIn" },
        new() { Command = "status", Description = "📋 Ver as configurações atuais" },
        new()
        {
            Command = "worktype",
            Description = "🏢 Definir o modelo de trabalho (Presencial, Remoto, Híbrido ou Todos)",
        },
    ];
}

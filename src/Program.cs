using JobScraperBot.Constants;
using JobScraperBot.Enums;
using JobScraperBot.Extensions;
using JobScraperBot.Interfaces;
using JobScraperBot.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    public static async Task Main(string[]? args)
    {
        string[] arguments = args ?? [];
        string? command = arguments.FirstOrDefault();

        IHost host = Host.CreateDefaultBuilder(arguments)
            .ConfigureEnvironment()
            .ConfigureLogging()
            .ConfigureContext()
            .ConfigureDependencies()
            .Build();

        IJobScraperService scraper = host.Services.GetRequiredService<IJobScraperService>();
        ITelegramBotService bot = host.Services.GetRequiredService<ITelegramBotService>();

        await scraper.StartAsync(CancellationToken.None);
        await bot.StartAsync(CancellationToken.None);

        if (command == "--cron")
        {
            _ = Enum.TryParse(EnvironmentVariables.CronWorkType, out WorkType workType);

            int? postedTime = int.TryParse(EnvironmentVariables.CronPostedTime, out int postedTimeValue) ? postedTimeValue : null;
            int limit = int.TryParse(EnvironmentVariables.CronLimit, out int limitValue) ? limitValue : 5;
            bool ignoredJobsFound = bool.TryParse(EnvironmentVariables.CronIgnoreJobsFound, out bool ignoredJobsFoundValue) && ignoredJobsFoundValue;

            User user = new()
            {
                Id = Guid.Parse(EnvironmentVariables.CronUserId),
                ChatId = EnvironmentVariables.CronChatId,
                WorkType = workType,
                PostedTime = postedTime,
                Limit = limit,
                Keywords = EnvironmentVariables.CronKeywords,
                IgnoreJobsFound = ignoredJobsFound
            };

            await scraper.RunJobSearchAsync(user, CancellationToken.None);
        }

        await host.RunAsync();
    }
}

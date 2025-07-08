using LinkJoBot.Constants;
using LinkJoBot.Entities;
using LinkJoBot.Extensions;
using LinkJoBot.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    public static async Task Main(string[]? args)
    {
        var arguments = args ?? [];
        var command = arguments.FirstOrDefault();

        var host = Host.CreateDefaultBuilder(arguments)
            .ConfigureEnvironment()
            .ConfigureContext()
            .ConfigureDependencies()
            .Build();

        var scraper = host.Services.GetRequiredService<IJobSearchService>();
        var bot = host.Services.GetRequiredService<IChatBotService>();

        await scraper.StartAsync(CancellationToken.None);
        await bot.StartAsync(CancellationToken.None);

        if (command == "--cron")
        {
            var user = new User()
            {
                Id = EnvironmentVariables.CronUserId,
                ChatId = EnvironmentVariables.CronChatId,
                WorkType = EnvironmentVariables.CronWorkType,
                PostedTime = EnvironmentVariables.CronPostedTime,
                Limit = EnvironmentVariables.CronLimit,
                Keywords = EnvironmentVariables.CronKeywords,
                IgnoreJobsFound = EnvironmentVariables.CronIgnoreJobsFound,
            };

            await scraper.RunJobSearchAsync(user, CancellationToken.None);
        }

        await host.RunAsync();
    }
}

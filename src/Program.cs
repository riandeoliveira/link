using LinkJobber.Constants;
using LinkJobber.Entities;
using LinkJobber.Extensions;
using LinkJobber.Interfaces;
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

        var jobSearchService = host.Services.GetRequiredService<IJobSearchService>();
        var chatBotHandlerService = host.Services.GetRequiredService<IChatBotHandlerService>();

        await jobSearchService.StartAsync(CancellationToken.None);
        await chatBotHandlerService.StartAsync(CancellationToken.None);

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

            await jobSearchService.RunJobSearchAsync(user, CancellationToken.None);
        }

        await host.RunAsync();
    }
}

using LinkJoBot.Constants;
using LinkJoBot.Interfaces;
using LinkJoBot.Repositories;
using LinkJoBot.SeedWork;
using LinkJoBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace LinkJoBot.Extensions;

public static class DependenciesExtensions
{
    public static IHostBuilder ConfigureDependencies(this IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // DB Context and Repositories
            services.AddScoped<IIgnoredJobRepository, IgnoredJobRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();

            // ChatBot client
            services.AddScoped<ITelegramBotClient>(
                (_) => new TelegramBotClient(EnvironmentVariables.ChatBotToken)
            );

            // Services
            services.AddScoped<IChatBotNotifierService, ChatBotNotifierService>();
            services.AddScoped<IChatBotService, ChatBotService>();
            services.AddScoped<IJobSearchService, JobSearchService>();
        });

        return builder;
    }
}

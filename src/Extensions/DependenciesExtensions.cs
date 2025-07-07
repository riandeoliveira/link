using JobScraperBot.Constants;
using JobScraperBot.Interfaces;
using JobScraperBot.Repositories;
using JobScraperBot.SeedWork;
using JobScraperBot.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Telegram.Bot;

namespace JobScraperBot.Extensions;

public static class DependenciesExtensions
{
    public static IHostBuilder ConfigureDependencies(this IHostBuilder builder)
    {
        builder.ConfigureServices((services) =>
        {
            services.AddScoped<IChatBotNotifierService, ChatBotNotifierService>();
            services.AddScoped<IIgnoredJobRepository, IgnoredJobRepository>();
            services.AddScoped<IJobScraperService, JobScraperService>();
            services.AddScoped<ITelegramBotClient>((_) => new TelegramBotClient(EnvironmentVariables.ChatBotToken));
            services.AddScoped<ITelegramBotService, TelegramBotService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
        });

        return builder;
    }
}

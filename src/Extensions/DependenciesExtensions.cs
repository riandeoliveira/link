using LinkJobber.Constants;
using LinkJobber.Interfaces;
using LinkJobber.Repositories;
using LinkJobber.SeedWork;
using LinkJobber.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace LinkJobber.Extensions;

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
            services.AddScoped<IChatBotHandlerService, ChatBotHandlerService>();
            services.AddScoped<IChatBotNotifierService, ChatBotNotifierService>();
            services.AddScoped<IJobSearchService, JobSearchService>();
        });

        return builder;
    }
}

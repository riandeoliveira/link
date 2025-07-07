using Microsoft.Extensions.Hosting;

using Serilog;

namespace JobScraperBot.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("../logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        return builder.UseSerilog();
    }
}

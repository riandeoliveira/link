using DotNetEnv;

using Microsoft.Extensions.Hosting;

namespace JobScraperBot.Extensions;

public static class EnvironmentExtensions
{
    public static IHostBuilder ConfigureEnvironment(this IHostBuilder builder)
    {
        string? envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");

        Env.Load(envPath);

        return builder;
    }
}

using DotNetEnv;
using Microsoft.Extensions.Hosting;

namespace LinkJoBot.Extensions;

public static class EnvironmentExtensions
{
    public static IHostBuilder ConfigureEnvironment(this IHostBuilder builder)
    {
        var envPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
            ".env"
        );

        Env.Load(envPath);

        return builder;
    }
}

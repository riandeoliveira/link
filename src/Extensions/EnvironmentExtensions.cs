using DotNetEnv;
using Microsoft.Extensions.Hosting;

namespace LinkJobber.Extensions;

public static class EnvironmentExtensions
{
    public static IHostBuilder ConfigureEnvironment(this IHostBuilder builder)
    {
        var envPath = Path.Combine(AppContext.BaseDirectory, ".env");

        Env.Load(envPath);

        return builder;
    }
}

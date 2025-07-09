using LinkJobber.Constants;
using LinkJobber.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkJobber.Extensions;

public static class ContextExtensions
{
    public static IHostBuilder ConfigureContext(this IHostBuilder builder)
    {
        builder.ConfigureServices(
            (context, services) =>
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(Database.ConnectionString)
                )
        );

        return builder;
    }
}

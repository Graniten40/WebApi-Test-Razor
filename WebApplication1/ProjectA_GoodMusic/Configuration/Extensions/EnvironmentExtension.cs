using Microsoft.Extensions.DependencyInjection;
using Configuration.Options;
using Microsoft.Extensions.Configuration;

namespace Configuration.Extensions;

public static class EnvironmentExtensions
{
    public static IServiceCollection AddEnvironmentInfo(this IServiceCollection serviceCollection)
    {
        var sp = serviceCollection.BuildServiceProvider();
        var configuration = sp.GetRequiredService<IConfiguration>();
        var databaseConnections = sp.GetService<DatabaseConnections>();

        serviceCollection.Configure<EnvironmentOptions>(options => EnvironmentOptions.ReadEnvironment(options, configuration, databaseConnections));
        return serviceCollection;
    }
}

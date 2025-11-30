using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Configuration.Options;

namespace Configuration.Extensions;

public static class DatabaseExtensions
{

    public static IServiceCollection AddDatabaseConnections(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<DbConnectionSetsOptions>(
            options => configuration.GetSection(DbConnectionSetsOptions.Position).Bind(options));
        serviceCollection.AddSingleton<DatabaseConnections>();

        return serviceCollection;
    }
}
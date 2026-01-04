using Microsoft.EntityFrameworkCore;                   
using Microsoft.Extensions.DependencyInjection;         
using Microsoft.Extensions.Configuration;             
using Microsoft.AspNetCore.Http;                        
using Microsoft.AspNetCore.Authentication;              

using Configuration;                                 
using Configuration.Options;                            
using Microsoft.Extensions.Options;                      
using Encryption;                                     

namespace DbContext.Extensions;

public static class DbContextExtensions
{
    // Registrerar en "user-based" MainDbContext:
    // dvs väljer connection string beroende på vem som anropar endpointen (UserRole från JWT)
    public static IServiceCollection AddUserBasedDbContext(this IServiceCollection serviceCollection)
    {
        // Gör HttpContext åtkomligt via DI (viktigt för att kunna läsa access_token)
        serviceCollection.AddHttpContextAccessor();

        // Registrerar MainDbContext.
        // Överlag: detta är en factory-lambda som får serviceProvider och options.
        // Du kan hämta config/services från DI och sedan sätta provider/connection.
        serviceCollection.AddDbContext<MainDbContext>((serviceProvider, options) =>
        {
            // Hämtar IConfiguration (appsettings/secrets/ENV)
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Hämtar dina DB-connection definitioner (troligen en registry över olika användare/roles)
            var databaseConnections = serviceProvider.GetRequiredService<DatabaseConnections>();

            // Hämtar options som beskriver miljön (vilken databastyp man kör osv)
            var environmentOptions = (serviceProvider.GetRequiredService<IOptions<EnvironmentOptions>>()).Value;

            // Default-roll om du INTE hittar user role från token.
            // Detta är din "fallback" när HttpContext saknas (t.ex. migrations, background jobs, etc.)
            var userRole = configuration["DatabaseConnections:DefaultDataUser"];

            // using jwt find out the user role requesting the endpoint
            // Hämtar JwtEncryptions från DI.
            // OBS: GetService kan returnera null (om ej registrerad) → du hanterar det senare.
            var jwtEncryptions = serviceProvider.GetService<JwtEncryptions>();

            // Hämtar HttpContext via accessor
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            // Om vi är i en HTTP-request och JwtEncryptions finns,
            // försök plocka access token och få ut claim "UserRole"
            if (httpContext != null && jwtEncryptions != null)
            {
                // Hämtar token från authentication systemet.
                // OBS: .Result blockerar tråden och kan orsaka deadlocks i vissa scenarion,
                // men i praktiken fungerar det ofta i ASP.NET Core eftersom sync context saknas.
                // Fortfarande en “riskpunkt” om det används i andra pipeline-sammanhang.
                var token = httpContext.GetTokenAsync("access_token").Result;

                // Om token finns, extrahera claims och plocka "UserRole"
                if (token != null)
                {
                    var claims = jwtEncryptions.GetClaimsFromToken(token);

                    // Här antar du att claims innehåller nyckeln "UserRole".
                    // Om den saknas får du troligen KeyNotFoundException.
                    userRole = claims["UserRole"];
                }
            }

            // Slår upp connection details baserat på userRole.
            // Tanken: olika roller => olika DB user / olika connection string / olika rättigheter.
            var conn = databaseConnections.GetDataConnectionDetails(userRole);

            // Väljer provider baserat på environmentOptions.DatabaseInfo.DataConnectionServer
            if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.SQLServer)
            {
                // SQL Server + transient retry (bra för moln/temporära nätfel)
                options.UseSqlServer(conn.DbConnectionString, options => options.EnableRetryOnFailure());
            }
            else if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.MySql)
            {
                // MySQL via Pomelo:
                // - AutoDetect server version från connection string
                // - SchemaBehavior Translate: byter schema/table naming med $"{schema}_{table}"
                //   (bra om du vill simulera schema i MySQL som saknar schema på samma sätt)
                options.UseMySql(conn.DbConnectionString, ServerVersion.AutoDetect(conn.DbConnectionString),
                    b => b.SchemaBehavior(
                        Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Translate,
                        (schema, table) => $"{schema}_{table}"
                    ));
            }
            else if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.PostgreSql)
            {
                // PostgreSQL via Npgsql
                options.UseNpgsql(conn.DbConnectionString);
            }
            else
            {
                // okänd databastyp -> fail fast
                throw new InvalidDataException($"DbContext for {environmentOptions.DatabaseInfo.DataConnectionServer} not existing");
            }
        });

        return serviceCollection;
    }
}

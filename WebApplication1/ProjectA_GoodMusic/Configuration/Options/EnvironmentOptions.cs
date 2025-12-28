using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Configuration.Options;

/// <summary>
/// Represents version and build information about the application
/// </summary>
public class EnvironmentOptions
{
    public record DatabaseInformation(
       string DataConnectionTag,
       string DefaultDataUser,
       string MigrationUser,
       DatabaseServer DataConnectionServer
    )
    {
        //for json clear text
        public string DataConnectionServerString => DataConnectionServer.ToString();  
    }

    private string _userSecretsId = null;

    public string AppEnvironment { get; private set; }
    public string SecretSource { get; private set; }
    public string SecretId => SecretSource switch
    {
        "AzureKeyVault" => $"{Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_kvSecret")}",
        _ => _userSecretsId
    };

    public DatabaseInformation DatabaseInfo { get; private set; }

    public static EnvironmentOptions ReadEnvironment(EnvironmentOptions options, IConfiguration _configuration, DatabaseConnections databaseConnections)
    {   
        var assembly = System.Reflection.Assembly.Load("Configuration");
        var userSecretsIdAttribute = assembly.GetCustomAttributes(typeof(UserSecretsIdAttribute), false)
            .FirstOrDefault() as UserSecretsIdAttribute;

        options.AppEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        options._userSecretsId = userSecretsIdAttribute?.UserSecretsId;
        options.SecretSource = _configuration.GetValue<string>("ApplicationSecrets:SecretStorage");

        options.DatabaseInfo = null;
        if (databaseConnections != null)
        {
            options.DatabaseInfo = new DatabaseInformation(
                databaseConnections.GetActiveDbSet.DbTag,
                _configuration["DatabaseConnections:DefaultDataUser"],
                _configuration["DatabaseConnections:MigrationUser"],
                databaseConnections.GetActiveDbSet.DbServer.Trim().ToLower() switch
                {
                    "sqlserver" => DatabaseServer.SQLServer,
                    "mysql" => DatabaseServer.MySql,
                    "postgresql" => DatabaseServer.PostgreSql,
                    "sqlite" => DatabaseServer.SQLite,
                    _ => throw new NotSupportedException($"DbServer {databaseConnections.GetActiveDbSet.DbServer} not supported")
                });
        }
        return options;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Configuration.Options;

namespace Configuration;

public enum DatabaseServer { SQLServer, MySql, PostgreSql, SQLite }

public class DatabaseConnections
{
    readonly IConfiguration _configuration;
    readonly DbConnectionSetsOptions _options;
    private readonly DbSetDetailOptions _activeDataSet;

    public DbSetDetailOptions GetActiveDbSet => _activeDataSet;

    public DbConnectionDetailOptions GetDataConnectionDetails(string user)
        => GetLoginDetails(user, _activeDataSet);

    DbConnectionDetailOptions GetLoginDetails(string user, DbSetDetailOptions dataSet)
    {
        if (string.IsNullOrWhiteSpace(user))
            throw new ArgumentNullException(nameof(user));

        if (dataSet is null)
            throw new ArgumentNullException(nameof(dataSet));

        if (dataSet.DbConnections is null || dataSet.DbConnections.Count == 0)
            throw new InvalidOperationException($"No DbConnections configured for dataset '{dataSet.DbTag}'.");

        // Find the connection "key" for this user
        var conn = dataSet.DbConnections.First(m =>
            m.DbUserLogin.Trim().ToLower() == user.Trim().ToLower());

        // DbConnection is a key into ConnectionStrings (your UserSecrets uses this pattern)
        var csKey = conn.DbConnection?.Trim();

        if (string.IsNullOrWhiteSpace(csKey))
            throw new InvalidOperationException(
                $"DbConnection key is missing for user '{user}' in dataset '{dataSet.DbTag}'.");

        // Try both standard GetConnectionString + direct access
        var cs = _configuration.GetConnectionString(csKey)
                 ?? _configuration[$"ConnectionStrings:{csKey}"];

        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                $"Connection string not found for key '{csKey}'. " +
                $"Expected 'ConnectionStrings:{csKey}' in appsettings/user-secrets.");

        return new DbConnectionDetailOptions
        {
            DbUserLogin = conn.DbUserLogin,
            DbConnection = csKey,
            DbConnectionString = cs
        };
    }

    public DatabaseConnections(IConfiguration configuration, IOptions<DbConnectionSetsOptions> dbSetOption)
    {
        _configuration = configuration;
        _options = dbSetOption.Value;

        var tag = configuration["DatabaseConnections:UseDataSetWithTag"];

        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("DatabaseConnections:UseDataSetWithTag is missing.");

        if (_options.DataSets is null || _options.DataSets.Count == 0)
            throw new ArgumentException("ConnectionSets:DataSets is missing or empty.");

        _activeDataSet = _options.DataSets
            .FirstOrDefault(ds => ds.DbTag?.Trim().ToLower() == tag.Trim().ToLower());

        if (_activeDataSet == null)
            throw new ArgumentException($"Dataset with DbTag {tag} not found");
    }
}

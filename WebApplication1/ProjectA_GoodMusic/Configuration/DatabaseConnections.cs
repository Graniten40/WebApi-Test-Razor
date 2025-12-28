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
    public DbConnectionDetailOptions GetDataConnectionDetails(string user) => GetLoginDetails(user, _activeDataSet);
    DbConnectionDetailOptions GetLoginDetails(string user, DbSetDetailOptions dataSet)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrWhiteSpace(user))
            throw new ArgumentNullException(nameof(user));

        var conn = dataSet.DbConnections.First(m => m.DbUserLogin.Trim().ToLower() == user.Trim().ToLower());
        return new DbConnectionDetailOptions
        {
            DbUserLogin = conn.DbUserLogin,
            DbConnection = conn.DbConnection,
            DbConnectionString = _configuration.GetConnectionString(conn.DbConnection)
        };
    }

    public DatabaseConnections(IConfiguration configuration, IOptions<DbConnectionSetsOptions> dbSetOption)
    {
        _configuration = configuration;
        _options = dbSetOption.Value;

        _activeDataSet = _options.DataSets.FirstOrDefault(ds => ds.DbTag.Trim().ToLower() == configuration["DatabaseConnections:UseDataSetWithTag"].Trim().ToLower());
        if (_activeDataSet == null)
            throw new ArgumentException($"Dataset with DbTag {configuration["DatabaseConnections:UseDataSetWithTag"]} not found");
    }
}
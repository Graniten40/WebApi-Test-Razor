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
    public static IServiceCollection AddUserBasedDbContext(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpContextAccessor();
        serviceCollection.AddDbContext<MainDbContext>((serviceProvider, options) => 
        { 
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseConnections = serviceProvider.GetRequiredService<DatabaseConnections>();
            var environmentOptions = (serviceProvider.GetRequiredService<IOptions<EnvironmentOptions>>()).Value;

            
            var userRole = configuration["DatabaseConnections:DefaultDataUser"];
            
            //using jwt find out the user role requesting the endpoint
            var jwtEncryptions = serviceProvider.GetService<JwtEncryptions>(); 
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>(); 

            var httpContext = httpContextAccessor.HttpContext; 
            if (httpContext != null && jwtEncryptions != null) 
            { 
                var token = httpContext.GetTokenAsync("access_token").Result;
                if (token != null)
                {
                    var claims = jwtEncryptions.GetClaimsFromToken(token);
                    userRole = claims["UserRole"]; 
                }
            } 
            
            var conn = databaseConnections.GetDataConnectionDetails(userRole);
            if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.SQLServer)
            {
                options.UseSqlServer(conn.DbConnectionString, options => options.EnableRetryOnFailure());
            }
            else if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.MySql)
            {
                options.UseMySql(conn.DbConnectionString,ServerVersion.AutoDetect(conn.DbConnectionString),
                    b => b.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}"));
            }
            else if (environmentOptions.DatabaseInfo.DataConnectionServer == DatabaseServer.PostgreSql)
            {
                options.UseNpgsql(conn.DbConnectionString);
            }
            else
            {
                //unknown database type
                throw new InvalidDataException($"DbContext for {environmentOptions.DatabaseInfo.DataConnectionServer} not existing");
            }
        });
        
        return serviceCollection;
    }
}
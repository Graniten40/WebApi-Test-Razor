using Microsoft.EntityFrameworkCore;              
using Microsoft.Extensions.DependencyInjection;   
using Microsoft.Extensions.Configuration;         
using Microsoft.Extensions.Hosting.Internal;      
using Configuration;                              
using Configuration.Extensions;                   
using Configuration.Options;                     
using Microsoft.Extensions.Options;               

namespace DbContext.Extensions;

public static class DbContextDesignTimeExtensions
{
    // Extension method för att konfigurera EF Core DbContextOptionsBuilder vid design-time.
    // Den här används typiskt av en DesignTimeDbContextFactory eller DbContext ctor
    // så att "dotnet ef migrations ..." kan få rätt connection string även när Program.cs inte körs.
    public static DbContextOptionsBuilder ConfigureForDesignTime(
        this DbContextOptionsBuilder optionsBuilder, 
        Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> databaseOptions)
    {
        // Printar logg till console vid design-time (visas i terminaln när du kör migrations)
        System.Console.WriteLine($"Executing DesignTimeConfigure...");
        
        // Skapar en minimal DI + config pipeline som efterliknar Program.cs
        var (configuration, databaseConnections) = CreateDesignTimeServices();
        
        // Hämtar ut rätt DB-connection detail (inkl connection string) baserat på config/secret setup
        var connection = GetDatabaseConnection(configuration, databaseConnections);

        // Använder delegate-parametern "databaseOptions" för att faktiskt konfigurera provider
        // Ex: optionsBuilder.UseSqlServer(connectionString) eller UseNpgsql etc.
        optionsBuilder = databaseOptions(optionsBuilder, connection.DbConnectionString);

        // Verbos loggning för att se att allt gick igenom + vilken user/tagg som används
        System.Console.WriteLine($"DesignTimeConfigure completed successfully");
        System.Console.WriteLine($"Proceeding with migration.");
        System.Console.WriteLine($"   User: {connection.DbUserLogin}");
        System.Console.WriteLine($"   Database connection: {connection.DbConnection}");
        
        return optionsBuilder;
    }

    // Bygger upp config + services manuellt, eftersom EF design-time inte kör din vanliga Program.cs.
    private static (IConfiguration configuration, DatabaseConnections databaseConnections) CreateDesignTimeServices()
    {
        // ASP.NET Core Program.cs har inte körts av EF design-time,
        // så du “bootstrapp:ar” config+DI här istället.

        // Läser var appsettings.json ligger.
        // EFC_AppSettingsFolder är ett environment variable du kan sätta när du kör "dotnet ef"
        // (om du vill peka på en viss folder).
        var appsettingsFolder = Environment.GetEnvironmentVariable("EFC_AppSettingsFolder")?? Directory.GetCurrentDirectory();
        System.Console.WriteLine($"   using appsettings.json in folder: {appsettingsFolder}");

        // Du kräver att appsettings.json finns, annars stoppar du migrations med FileNotFoundException.
        if (File.Exists(Path.Combine(appsettingsFolder, "appsettings.json")))
        {
            System.Console.WriteLine($"   appsettings.json: {Path.Combine(appsettingsFolder, "appsettings.json")}");
        }
        else
        {
            throw new FileNotFoundException($"Error: appsettings.json not found in folder: {appsettingsFolder}");
        }

        // Skapar en configuration builder
        System.Console.WriteLine($"   configuring");
        var conf = new ConfigurationBuilder();

        // Din egna extension: AddSecrets(...)
        // - Du matar in en HostingEnvironment med EnvironmentName="Development"
        // - Plus foldern där appsettings.json ligger
        //
        // Notera: Microsoft.Extensions.Hosting.Internal.HostingEnvironment är en *intern* typ.
        // Det funkar men är lite “fragile” vid uppgraderingar eftersom den inte är public API.
        conf.AddSecrets(new HostingEnvironment { EnvironmentName = "Development" }, appsettingsFolder);

        System.Console.WriteLine($"   building configuring");
        var configuration = conf.Build();

        // Skapar en ServiceCollection och registrerar samma saker som du normalt gör i Program.cs
        System.Console.WriteLine($"   creating services");
        var serviceCollection = new ServiceCollection();

        // Lägger till Options-systemet (IOptions<T>)
        serviceCollection.AddOptions();

        // Dina egna extension methods:
        // - AddDatabaseConnections(configuration): registrerar DatabaseConnections
        // - AddSingleton<IConfiguration>(configuration): gör IConfiguration injicerbar
        // - AddEnvironmentInfo(): registrerar IOptions<EnvironmentOptions> eller liknande
        serviceCollection.AddDatabaseConnections(configuration);
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddEnvironmentInfo();

        // Bygger ServiceProvider för att kunna resolve:a services direkt här
        System.Console.WriteLine($"   retrieving services from serviceProvider");
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Hämtar IConfiguration tillbaka ur DI (ska matcha "configuration" du byggde)
        var configurationService = serviceProvider.GetRequiredService<IConfiguration>();
        System.Console.WriteLine($"   {nameof(IConfiguration)} retrieved");

        // Hämtar DatabaseConnections ur DI
        var databaseConnections = serviceProvider.GetRequiredService<DatabaseConnections>();

        // Hämtar EnvironmentOptions via IOptions<T> (options-pattern)
        // och tar .Value (själva objektet)
        var environmentOptions = (serviceProvider.GetRequiredService<IOptions<EnvironmentOptions>>()).Value;

        // Loggar lite för att se var hemligheter kommer ifrån
        System.Console.WriteLine($"   {nameof(DatabaseConnections)} retrieved");
        System.Console.WriteLine($"   secret source: {environmentOptions.SecretSource}");
        System.Console.WriteLine($"   secret id: {environmentOptions.SecretId}");
        System.Console.WriteLine($"   DataConnectionTag: {environmentOptions.DatabaseInfo.DataConnectionTag}");

        return (configurationService, databaseConnections);
    }

    // Slår upp vilken connection som ska användas för migration-usern
    private static DbConnectionDetailOptions GetDatabaseConnection(IConfiguration configuration, DatabaseConnections databaseConnections)
    {
        // Läser config-nyckeln "DatabaseConnections:MigrationUser"
        // och använder den som “lookup key” i DatabaseConnections
        var connection = databaseConnections.GetDataConnectionDetails(configuration["DatabaseConnections:MigrationUser"]);

        // Säkerhet: om connection string inte är satt, stoppa tidigt med tydligt fel
        if (connection.DbConnectionString == null)
        {
            throw new InvalidDataException($"Error: Connection string for {connection.DbConnection}, {connection.DbUserLogin} not set");
        }

        return connection;
    }
}

using Services;
using Configuration.Extensions;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

//adding support for several secret sources and database sources
//to use either user secrets or azure key vault depending on UseAzureKeyVault tag in appsettings.json
builder.Configuration.AddSecrets(builder.Environment);
Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
Console.WriteLine("DataService:WebApiBaseUri = " + builder.Configuration["DataService:WebApiBaseUri"]);


//use encryption and multiple Database connections and their respective DbContexts
builder.Services.AddEncryptions(builder.Configuration);
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddUserBasedDbContext();

// adding version and environment info
builder.Services.AddVersionInfo();
builder.Services.AddEnvironmentInfo();

#region Injecting a dependency service to read MusicWebApi
builder.Services.AddHttpClient(name: "MusicWebApi", configureClient: options =>
{
    options.BaseAddress = new Uri(builder.Configuration["DataService:WebApiBaseUri"]);
    options.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            mediaType: "application/json",
            quality: 1.0));
});


//Inject DbRepos and Services
builder.Services.AddScoped<AdminDbRepos>();
builder.Services.AddScoped<MusicGroupsDbRepos>();
builder.Services.AddScoped<AlbumsDbRepos>();
builder.Services.AddScoped<ArtistsDbRepos>();


builder.Services.AddSingleton<IMusicServiceActive, MusicServiceActive>();

//Inject a Service based on active data source, 
//E.g. DI will resolve either AdminServiceWapi or AdminServiceDb based on the active data source 
//and inject it wherever IAdminService is required
builder.Services.AddScoped<IAdminService> (sp => sp.GetService<IMusicServiceActive> ().ActiveDataSource switch
    {
        MusicDataSource.WebApi => ActivatorUtilities.CreateInstance<AdminServiceWapi>(sp),
        _ => ActivatorUtilities.CreateInstance<AdminServiceDb>(sp),
    });
builder.Services.AddScoped<IMusicGroupsService> (sp => sp.GetService<IMusicServiceActive> ().ActiveDataSource switch
    {
        MusicDataSource.WebApi => ActivatorUtilities.CreateInstance<MusicGroupsServiceWapi>(sp),
        _ => ActivatorUtilities.CreateInstance<MusicGroupsServiceDb>(sp),
    });
builder.Services.AddScoped<IAlbumsService> (sp => sp.GetService<IMusicServiceActive> ().ActiveDataSource switch
    {
        MusicDataSource.WebApi => ActivatorUtilities.CreateInstance<AlbumsServiceWapi>(sp),
        _ => ActivatorUtilities.CreateInstance<AlbumsServiceDb>(sp),
    });
builder.Services.AddScoped<IArtistsService> (sp => sp.GetService<IMusicServiceActive> ().ActiveDataSource switch
    {
        MusicDataSource.WebApi => ActivatorUtilities.CreateInstance<ArtistsServiceWapi>(sp),
        _ => ActivatorUtilities.CreateInstance<ArtistsServiceDb>(sp),
    });
#endregion

var app = builder.Build();

//using Hsts and https to secure the site
if (!app.Environment.IsDevelopment())
{
    //https://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security
    //https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
    app.UseHsts();
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();

//Use static files css, html, and js
app.UseStaticFiles();

//Use endpoint routing
app.UseRouting();

//Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

//This is the endpoint, Map Razorpages into Pages folder
app.MapRazorPages();

//Mapped Get response example
app.MapGet("/hello", () =>
{
    //read the environment variable ASPNETCORE_ENVIRONMENT
    //Change in launchSettings.json, (not VS2022 Debug/Release)
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var envMyOwn = Environment.GetEnvironmentVariable("MyOwn");

    return $"Hello World!\nASPNETCORE_ENVIRONMENT: {env}\nMyOwn: {envMyOwn}";
});

app.Run();




using Microsoft.EntityFrameworkCore;
using prueba.Components;
using prueba.Configuration;
using prueba.Data;
using prueba.Services;

var builder = WebApplication.CreateBuilder(args);

// NOTE: Set the connection string value in appsettings.json (ConnectionStrings:DefaultConnection)
// For Azure SQL Database put your full connection string there. It's intentionally left empty for now.

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure logging to console so logs are visible in the terminal / VS Output
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

// Register EF Core DbContext and application services
// Bind Cosmos settings to a typed options class from the ConnectionStrings2 section
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("ConnectionStrings2"));

// Register two DbContext implementations:
// - AppDbContext is implemented by SqlAppDbContext (default for existing code)
// - CosmosAppDbContext is available when you need to work with Cosmos-specific features
var cosmosSettings = builder.Configuration.GetSection("ConnectionStrings2").Get<CosmosSettings>();
var cosmosConn = cosmosSettings?.CosmosDb;
var cosmosDbName = cosmosSettings?.CosmosDb_Database;

// Register AppDbContext -> SqlAppDbContext (default)
builder.Services.AddDbContext<AppDbContext, SqlAppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register Cosmos-specific context
builder.Services.AddDbContext<CosmosAppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(cosmosConn))
    {
        string? endpoint = null;
        string? key = null;
        foreach (var part in cosmosConn.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            var k = kv[0].Trim();
            var v = kv[1].Trim();
            if (k.Equals("AccountEndpoint", StringComparison.OrdinalIgnoreCase)) endpoint = v;
            if (k.Equals("AccountKey", StringComparison.OrdinalIgnoreCase)) key = v;
        }

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(key))
        {
            var dbName = !string.IsNullOrWhiteSpace(cosmosDbName) ? cosmosDbName : "MyCosmosDatabase";
            options.UseCosmos(endpoint, key, dbName);
        }
    }
});

// Register CosmosClient for direct SDK access (used by ParticipantsService)
if (!string.IsNullOrWhiteSpace(cosmosConn))
{
    string? endpoint = null;
    string? key = null;
    foreach (var part in cosmosConn.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        var kv = part.Split('=', 2);
        if (kv.Length != 2) continue;
        var k = kv[0].Trim();
        var v = kv[1].Trim();
        if (k.Equals("AccountEndpoint", StringComparison.OrdinalIgnoreCase)) endpoint = v;
        if (k.Equals("AccountKey", StringComparison.OrdinalIgnoreCase)) key = v;
    }

    if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(key))
    {
        builder.Services.AddSingleton(new Microsoft.Azure.Cosmos.CosmosClient(endpoint, key));
    }
}

builder.Services.AddScoped<IWeatherService, WeatherService>();
// Register ParticipantsService which always reads from Cosmos
builder.Services.AddScoped<IParticipantsService, ParticipantsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

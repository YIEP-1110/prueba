using Microsoft.EntityFrameworkCore;
using prueba.Components;
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
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Uses the DefaultConnection value from appsettings.json. Keep it empty for now as requested.
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IWeatherService, WeatherService>();

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

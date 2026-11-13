using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;
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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acceso-denegado";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

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


// Register Cosmos-specific context


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



builder.Services.AddScoped<IParticipantesCrudService, ParticipantesCrudService>();
builder.Services.AddScoped<IPonenciasCrudService, PonenciasCrudService>();
builder.Services.AddScoped<IPonentesCrudService, PonentesCrudService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IPdfStorageService, LocalPdfStorageService>();
builder.Services.AddScoped<IPonenteActualService, PonenteActualService>();  
builder.Services.AddCascadingAuthenticationState();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapPost("/account/login", async (HttpRequest request, IUsuariosService usuarios, HttpContext http, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(http);
    var form = await request.ReadFormAsync();
    var correo = form["correo"].ToString();
    var contraseña = form["contraseña"].ToString();
    var resultado = await usuarios.AutenticarAsync(correo, contraseña);

    if (!resultado.Correcta || resultado.Usuario is null)
        return Results.Redirect($"/login?error={Uri.EscapeDataString("Correo o contraseña incorrectos.")}");

    var usuario = resultado.Usuario;
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, usuario.IdUsuario),
        new(ClaimTypes.Name, usuario.Correo),
        new(ClaimTypes.Email, usuario.Correo),
        new(ClaimTypes.Role, usuario.Rol.Tipo)
    };
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
        new AuthenticationProperties { IsPersistent = true });

    return Results.Redirect("/");
});

app.MapPost("/account/register", async (HttpRequest request, IUsuariosService usuarios, HttpContext http, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(http);
    var form = await request.ReadFormAsync();
    var resultado = await usuarios.RegistrarParticipanteAsync(
        form["correo"].ToString(), form["contraseña"].ToString());

    if (!resultado.Creado)
        return Results.Redirect($"/registro?error={Uri.EscapeDataString(resultado.Error ?? "No se pudo completar el registro.")}");

    return Results.Redirect("/login?registro=true");
});

app.MapPost("/account/logout", async (HttpContext http, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(http);
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

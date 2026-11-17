using System.Globalization;
using FinanceTrack.Application;
using FinanceTrack.Infrastructure;
using FinanceTrack.Infrastructure.Data;
using FinanceTrack.Web.Components;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

// Ensure consistent culture for monetary/numeric formatting and EF Core SQLite collations
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Static web assets manifest is only available in Development (SDK environment).
// In Production (Docker/Render), published files in wwwroot/_framework are served by UseStaticFiles().
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

// Configure forwarded headers for reverse proxy environments (e.g. Render, Docker, Cloud)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Support dynamic PORT environment variable (e.g. Render assigns PORT dynamically)
var customPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(customPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{customPort}");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Clean Architecture Service Registrations
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Forwarded headers must be processed first behind reverse proxies
app.UseForwardedHeaders();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceTrackDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// In .NET 9+, MapStaticAssets maps the static assets manifest including _framework/blazor.web.js.
// UseStaticFiles serves any disk files in wwwroot.
app.MapStaticAssets();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program { }

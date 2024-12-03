using Microsoft.EntityFrameworkCore;
using MVC.Models;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuration manuel pour prendre la connectionString du AppConfig hardcoder dans le appsettings.json
// Cette information pourrait être passer via une variable d'environement ou encore mieux, utiliser le endpoint et le defaultazurecredential comme dans l'exemple plus bas.
// https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app?tabs=entra-id
string connectionString = builder.Configuration.GetConnectionString("AppConfig")!;

// Meilleur option ici en utilisant Microsoft EntraID pour ce connecter via l'endpoint ainsi nous n'avons aucun secrets "exposed"
// string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

// Initialize AppConfig
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
    // Besoin du "App Configuration Data Reader" role
    // options.Connect(new Uri(AppConfigEndPoint), new DefaultAzureCredential());

    // Ajout de la configuration du sentinel pour rafraichir la configuration si il y a changement
    // https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core
    .Select("*")

    // Requis pour l'ajout des Feature Flag ...
    // https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core
    .UseFeatureFlags()

    .ConfigureRefresh(refreshOptions =>
    refreshOptions.Register("ApplicationConfiguration:Sentinel", refreshAll: true)
        .SetRefreshInterval(new TimeSpan(0,0,10)));

    options.ConfigureKeyVault(keyVaultOptions =>
    {
        // Besoin du "Key Vault Secrets Officer" role
        keyVaultOptions.SetCredential(new DefaultAzureCredential());
    });
});

// ajout du service middleware pour AppConfig et FeatureFlag
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

// Liaison de la Configuration "ApplicationConfiguration" a la class
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

// Application Insight Service
// https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore
builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsight")!;
});

// Ajouter la BD
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalSQL")!)
    .LogTo(Console.WriteLine, LogLevel.Trace)
    .EnableDetailedErrors());

var app = builder.Build();

// Utilise le middleware de AppConfig pour rafraichir la configuration dynamique.
app.UseAzureAppConfiguration();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
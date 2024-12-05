using Microsoft.EntityFrameworkCore;
using MVC.Models;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.FeatureManagement;
using MVC.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cette information pourrait être passer via une variable d'environement ou encore mieux, utiliser le endpoint et le defaultazurecredential comme dans l'exemple plus bas.
// https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app?tabs=entra-id

// Meilleur option ici en utilisant Microsoft EntraID pour ce connecter via l'endpoint ainsi nous n'avons aucun secrets "exposed"
string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

// Initialize AppConfig
builder.Configuration.AddAzureAppConfiguration(options =>
{
    // Besoin du "App Configuration Data Reader" role
    options.Connect(new Uri(AppConfigEndPoint), new DefaultAzureCredential())

    // Ajout de la configuration du sentinel pour rafraichir la configuration si il y a changement
    // https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core
    .Select("*")

    // Requis pour l'ajout des Feature Flag ...
    // https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core
    .UseFeatureFlags()

    .ConfigureRefresh(refreshOptions =>
    refreshOptions.Register("ApplicationConfiguration:Sentinel", refreshAll: true)
        .SetRefreshInterval(new TimeSpan(0, 0, 10)));

    options.ConfigureKeyVault(keyVaultOptions =>
    {
        // Besoin du "Key Vault Secrets Officer" role
        keyVaultOptions.SetCredential(new DefaultAzureCredential());
    });
});

// Ajout du service middleware pour AppConfig et FeatureFlag
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

// Liaison de la Configuration "ApplicationConfiguration" a la class
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

// Application Insight Service & OpenTelemetry
// https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsight")!;
});

//Ajouter la BD ( SQL ou NoSQL )

builder.Services.AddDbContext<ApplicationDbContextSQL>();

//builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//    options
//        .UseCosmos(
//            connectionString: "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
//            databaseName: "ApplicationDB",
//            cosmosOptionsAction: options =>
//            {
//                options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
//                options.MaxRequestsPerTcpConnection(16);
//                options.MaxTcpConnectionsPerEndpoint(32);
//            }));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    //Efface la Database a chaque usage
    // If No SQL deactivate this
     dbContext.Database.EnsureDeleted();

    //Migration des Schema
     dbContext.Database.Migrate();
}

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
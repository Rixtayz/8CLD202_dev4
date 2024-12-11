using Microsoft.EntityFrameworkCore;
using MVC.Models;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.FeatureManagement;
using MVC.Data;
using MVC.Business;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cette information pourrait �tre passer via une variable d'environement ou encore mieux, utiliser le endpoint et le defaultazurecredential comme dans l'exemple plus bas.
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
        .SetRefreshInterval(new TimeSpan(0, 0, 30)));

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

// Pour rebuild des database SQL
//builder.Services.AddDbContext<ApplicationDbContextSQL>();
//builder.Services.AddScoped<IRepository, EFRepositorySQL>();

// Ajouter la BD ( SQL ou NoSQL )
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        builder.Services.AddDbContext<ApplicationDbContextSQL>();
        builder.Services.AddScoped<IRepository, EFRepositorySQL>();
        break;

    case "NoSQL":
        builder.Services.AddDbContext<ApplicationDbContextNoSQL>();
        builder.Services.AddScoped<IRepository, EFRepositoryNoSQL>();
        break;

    case "InMemory":
        builder.Services.AddDbContext<ApplicationDbContextInMemory>();
        builder.Services.AddScoped<IRepository, EFRepositoryInMemory>();
        break;
}

// Ajouter le BlobController du BusinessLayer dans nos Injection de d�pendance
builder.Services.AddScoped<BlobController>();

// Ajout de l'authentification 
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContextInMemory>();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContextInMemory>();

builder.Services.AddRazorPages();


var app = builder.Build();

// Pour rebuild des database SQL
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSQL>();

//    dbContext.Database.EnsureDeleted();
//    dbContext.Database.Migrate();
//}

// Configuration de la BD ( SQL ou NoSQL )
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSQL>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        }
        break;

    case "NoSQL":
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContextNoSQL>();
            await context.Database.EnsureCreatedAsync();
        }
        break;
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


// Identity
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
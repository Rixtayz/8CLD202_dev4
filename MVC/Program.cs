using Microsoft.EntityFrameworkCore;
using MVC.Models;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using NuGet.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string This one is local to the project ... could be pass as environment variable.
string connectionString = builder.Configuration.GetConnectionString("AppConfig")!;

// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString);
    options.ConfigureKeyVault(keyVaultOptions =>
    {
        keyVaultOptions.SetCredential(new DefaultAzureCredential());
    });
});

string test1 = builder.Configuration.GetConnectionString("ApplicationInsightsTest")!;
string test2 = builder.Configuration.GetConnectionString("LocalSQLTest")!;


// Application Insight Service
// https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore
builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights")!;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Ajouter la BD
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalSQL")!.Replace(@"\\",@"\"))  // not sure why, but AppConfig or AzureKey double escape that thing.
    .LogTo(Console.WriteLine, LogLevel.Trace)
    .EnableDetailedErrors());

var app = builder.Build();

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
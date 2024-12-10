using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Data;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.FeatureManagement;
using MVC.Business;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Lecture du AppConfig Endpoint
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

//Add DbContext
// Ajouter la BD ( SQL ou NoSQL )
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        builder.Services.AddDbContext<ApplicationDbContextSQL>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPI<ApplicationDbContextSQL>>();
        break;

    case "NoSQL":
        builder.Services.AddDbContext<ApplicationDbContextNoSQL>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPINoSQL>();
        break;

    case "InMemory":
        builder.Services.AddDbContext<ApplicationDbContextInMemory>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPI<ApplicationDbContextInMemory>>();
        break;
}
// Ajouter le service pour Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.OperationFilter<FileUploadOperationFilter>() // Add custom operation filter, Ceci est pour le FileUpload
);


// Ajouter le BlobController du BusinessLayer dans nos Injection de dépendance
builder.Services.AddScoped<BlobController>();

var app = builder.Build();

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
if (app.Environment.IsDevelopment())
{ 
    // Configuration des services Swagger
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//API Specific
//Post
app.MapGet("/Posts/", async (IRepositoryAPI repo) => await repo.GetAPIPostsIndex());
app.MapGet("/Posts/{id}", async (IRepositoryAPI repo, Guid id) => await repo.GetAPIPost(id));

// https://andrewlock.net/reading-json-and-binary-data-from-multipart-form-data-sections-in-aspnetcore/
// J'ai laisser cette fonction la car je voulais m'assurer de la séparation des concern, sinon j'aurais ajouté de la logique business dans le data layer.
app.MapPost("/Posts/Add", async (IRepositoryAPI repo, [FromForm] PostCreateDTO post, BlobController blob) =>
{
    try
    {
        Guid guid = Guid.NewGuid();
        string Url = await blob.PushImageToBlob(post.Image, guid);
        Post Post = new Post { Title = post.Title, Category = post.Category, User = post.User, BlobImage = guid, Url = Url };
        return await repo.CreateAPIPost(Post);
    }
    catch (ExceptionFilesize)
    {
        return TypedResults.BadRequest();
    }
    // DisableAntiforgery car .net 9.0 l'ajoute automatiquement.
}).DisableAntiforgery();

app.MapPost("/Posts/IncrementPostLike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementPostLike(id));
app.MapPost("/Posts/IncrementPostDislike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementPostDislike(id));

//Comment
//Id or PostId ( va retourner 1 ou plusieurs comments)
app.MapGet("/Comments/{id}", async (IRepositoryAPI repo, Guid id) => await repo.GetAPIComment(id));

app.MapPost("/Comments/Add", async (IRepositoryAPI repo, Comment comment) => await repo.CreateAPIComment(comment));
app.MapPost("/Comments/IncrementCommentLike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementCommentLike(id));
app.MapPost("/Comments/IncrementCommentsDislike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementCommentDislike(id));

app.Run();
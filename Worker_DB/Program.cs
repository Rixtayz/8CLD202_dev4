using Worker_DB;

//Event Hub
using Azure.Messaging.EventHubs;
using System.Collections.Concurrent;

//Azure Identity
using Azure.Identity;

//App Config
using Azure.Data.AppConfiguration;

//KeyVault
using Azure.Security.KeyVault.Secrets;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Code différent pour le Azure.Data.AppConfiguration
string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

// Option pour le credential recu des variables d'environement.
DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeSharedTokenCacheCredential = true,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeEnvironmentCredential = false
});

// Création du Client App Config
ConfigurationClient appConfigClient = new ConfigurationClient(new Uri(AppConfigEndPoint), defaultAzureCredential);

// Création du Client Key Vault
ConfigurationSetting endpointKeyVault = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault");
SecretClient keyVaultClient = new SecretClient(new Uri(endpointKeyVault.Value), defaultAzureCredential);

KeyVaultSecret EventHubString = keyVaultClient.GetSecret("ConnectionStringBlob");
KeyVaultSecret applicationinsightKeyVault = keyVaultClient.GetSecret("ConnectionStringApplicationInsight");

var host = builder.Build();
host.Run();

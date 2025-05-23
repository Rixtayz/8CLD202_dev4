using Azure.Security.KeyVault.Secrets;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MVC.Business;

namespace Worker_Content
{
    public class Worker_Content
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            // Code diff�rent pour le Azure.Data.AppConfiguration
            string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

            // Option pour le credential recu des variables d'environement.
            DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeEnvironmentCredential = false
            });

            // Cr�ation du Client App Config
            ConfigurationClient appConfigClient = new ConfigurationClient(new Uri(AppConfigEndPoint), defaultAzureCredential);
            ConfigurationSetting contentsafetyEndPoint = appConfigClient.GetConfigurationSetting("Endpoints:ContentSafety");
            ConfigurationSetting container1 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:UnvalidatedBlob");
            ConfigurationSetting container2 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:ValidatedBlob");
            ConfigurationSetting ServiceBusQueue2Name = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:ServiceBusQueue2Name");
            ConfigurationSetting EventHubHubName = appConfigClient.GetConfigurationSetting("\r\nApplicationConfiguration:EventHubName");


            // Cr�ation du Client Key Vault
            ConfigurationSetting endpointKeyVault = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault");
            SecretClient keyVaultClient = new SecretClient(new Uri(endpointKeyVault.Value), defaultAzureCredential);

            KeyVaultSecret blobKeyVault = keyVaultClient.GetSecret("ConnectionStringBlob");
            KeyVaultSecret servicebusKeyVault = keyVaultClient.GetSecret("ConnectionStringSB");
            KeyVaultSecret applicationinsightKeyVault = keyVaultClient.GetSecret("ConnectionStringApplicationInsight");
            KeyVaultSecret EventHubKey = keyVaultClient.GetSecret("ConnectionStringEventHub");
            KeyVaultSecret contentsafetyKeyVault = keyVaultClient.GetSecret("ConnectionStringContentSafety");

            // Ajout de secrets a la configuration du worker
            builder.Services.Configure<WorkerOptions>(options =>
            {
                options.BlobStorageKey = blobKeyVault.Value;
                options.BlobContainer1 = container1.Value;
                options.BlobContainer2 = container2.Value;
                options.ServiceBusKey = servicebusKeyVault.Value;
                options.EventHubKey = EventHubKey.Value;
                options.EventHubHubName = EventHubHubName.Value;
                options.ContentSafetyKey = contentsafetyKeyVault.Value;
                options.ContentSafetyEndpoint = contentsafetyEndPoint.Value;
                options.ServiceBusQueue2Name = ServiceBusQueue2Name.Value;
            });

            // Application Insight trace/log/metrics
            // https://medium.com/@chuck.beasley/how-to-instrument-a-net-5537ea851763
            builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Worker_Content", Environment.GetEnvironmentVariable("HOSTNAME")!));
            builder.Services.AddLogging(logging =>
            {
                logging.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) =>
                    config.ConnectionString = applicationinsightKeyVault.Value,
                    configureApplicationInsightsLoggerOptions: (options) => { }
                    );
                logging.AddFilter<ApplicationInsightsLoggerProvider>("Worker_Content", LogLevel.Trace);

            });
            builder.Services.AddApplicationInsightsTelemetryWorkerService();
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
                o.ConnectionString = applicationinsightKeyVault.Value;
            });

            var host = builder.Build();
            host.Run();
        }
    }

    public class WorkerOptions
    {
        public required string BlobStorageKey { get; set; }
        public required string BlobContainer1 { get; set; }
        public required string BlobContainer2 { get; set; }
        public required string ServiceBusKey { get; set; }
        public required string EventHubKey { get; set; }

        public required string EventHubHubName { get; set; }
        public required string ContentSafetyKey { get; set; }
        public required string ContentSafetyEndpoint { get; set; }
        public required string ServiceBusQueue2Name { get; set; }
    }
}

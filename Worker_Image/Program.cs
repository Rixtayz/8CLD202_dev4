using Azure.Security.KeyVault.Secrets;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;

namespace Worker_Image
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            // Code différent pour le Azure.Data.AppConfiguration
            string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

            // Création du Client App Config
            ConfigurationClient appConfigClient = new ConfigurationClient(AppConfigEndPoint);
            ConfigurationSetting container1 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:UnvalidatedBlob");
            ConfigurationSetting container2 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:ValidatedBlob");

            // Création du Client Key Vault
            ConfigurationSetting endpointKeyVault = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault");
            SecretClient keyVaultClient = new SecretClient(new Uri(endpointKeyVault.Value), new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeEnvironmentCredential = false
            }));

            KeyVaultSecret blobKeyVault = keyVaultClient.GetSecret("ConnectionStringBlob");
            KeyVaultSecret servicebusKeyVault = keyVaultClient.GetSecret("ConnectionStringSB");
            KeyVaultSecret applicationinsightKeyVault = keyVaultClient.GetSecret("ConnectionStringApplicationInsight");
            KeyVaultSecret cosmosdbKeyVault = keyVaultClient.GetDeletedSecret("ConnectionStringCosmosDB");

            // Ajout de secrets a la configuration du worker
            builder.Services.Configure<WorkerOptions>(options =>
            {
                options.BlobStorageKey = blobKeyVault.Value;
                options.BlobContainer1 = container1.Value;
                options.BlobContainer2 = container2.Value;
                options.ServiceBusKey = servicebusKeyVault.Value;
                options.CosmosDbKey = cosmosdbKeyVault.Value;
            });

            // Application Insight trace/log/metrics
            // https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable?tabs=net#enable-azure-monitor-opentelemetry-for-net-nodejs-python-and-java-applications

            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddAzureMonitorTraceExporter(options => options.ConnectionString = applicationinsightKeyVault.Value);

            var metricsProvider = Sdk.CreateMeterProviderBuilder()
                .AddAzureMonitorMetricExporter(options => options.ConnectionString = applicationinsightKeyVault.Value);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    logging.AddAzureMonitorLogExporter(options => options.ConnectionString = applicationinsightKeyVault.Value);
                });
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
        public required string CosmosDbKey { get; set; }
    }
}




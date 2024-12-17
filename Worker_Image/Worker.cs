using Azure.Core;

//DI
using Microsoft.Extensions.Options;

//Service Bus
using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;

//Blob
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

//Images
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

//Serialization
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace Worker_Image
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly ConcurrentQueue<ServiceBusReceivedMessage> _messageQueue;
        private readonly WorkerOptions _options;
        private readonly SemaphoreSlim _semaphore;
        private readonly BlobServiceClient _blobServiceClient;
        private CosmosClient _cosmosClient;
        private Container _container;

        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            // CosmosDb ...
            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
            {
                MaxRetryAttemptsOnRateLimitedRequests = 9,      // MaxRetryAttemptsOnThrottledRequests: Maximum number of retry attempts on throttled requests
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),   // MaxRetryWaitTimeOnThrottledRequests: Maximum wait time for the retry attempts
                RequestTimeout = TimeSpan.FromSeconds(60),      // RequestTimeout: Sets the request timeout for network operations
                ConnectionMode = ConnectionMode.Direct,         // ConnectionMode: Use Direct mode for better performance and Gateway mode for improved resilience
                EnableTcpConnectionEndpointRediscovery = true   // EnableTcpConnectionEndpointRediscovery: Enable endpoint rediscovery in the case of connection failures
            };

            _cosmosClient = new CosmosClient(_options.CosmosDbKey, cosmosClientOptions);

            // Pour créer la connection au CosmosDB nous devons avoir le DatabaseId et le ContainerID, étant donner que 
            // Ceux-ci sont créer par le MVC lors de sont initialization, nous n'avons pas cette information.
            // Nous pouvons donc lister les ID, et assumer que ce sont les premier. Nous devrons également s'assurer que 
            // notre worker service démarre un coup ceux-ci créer pour ne pas avoir de problème de concurence.

            string databaseId = _cosmosClient.CreateDatabaseIfNotExistsAsync("ApplicationDB").Result.Database.Id;
            
            var database = _cosmosClient.GetDatabase(databaseId);

            var containerId = database.CreateContainerIfNotExistsAsync("Posts","/id").Result.Container.Id;
            //var containerId = database.CreateContainerIfNotExistsAsync("Comments","/PostId").Result.Container.Id;

            _container = _cosmosClient.GetContainer(databaseId, containerId);

            // Blob ...
            BlobClientOptions blobClientOptions = new BlobClientOptions
            {
                Retry = {
                        Delay = TimeSpan.FromSeconds(2),     //The delay between retry attempts for a fixed approach or the delay on which to base
                                                             //calculations for a backoff-based approach
                        MaxRetries = 5,                      //The maximum number of retry attempts before giving up
                        Mode = RetryMode.Exponential,        //The approach to use for calculating retry delays
                        MaxDelay = TimeSpan.FromSeconds(10)  //The maximum permissible delay between retry attempts
                        },
            };

            BlobServiceClient _blobServiceClient = new BlobServiceClient(_options.BlobStorageKey, blobClientOptions);

            // Service Bus ...
            _messageQueue = new ConcurrentQueue<ServiceBusReceivedMessage>();

            // Hardcoded
            string queueName = "imageresizemessage";

            ServiceBusClientOptions clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(10),
                    MaxDelay = TimeSpan.FromSeconds(60),
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 6,
                },
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                ConnectionIdleTimeout = TimeSpan.FromMinutes(10)   //Défault = 1 minutes
            };

            _serviceBusClient = new ServiceBusClient(_options.ServiceBusKey, clientOptions);
            _processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            _semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent messages
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            await _semaphore.WaitAsync();
            _messageQueue.Enqueue(args.Message);

            _ = ProcessMessagesAsync(args);
        }
        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try
            {
                // Nous avions passé un Tuple ici lors de la sérialization ...
                var message = JsonSerializer.Deserialize<Tuple<string, Guid>>(args.Message.Body.ToString());
                
                
                _logger.LogInformation($"Processing message: {args.Message.MessageId}");

                // Travail sur l'image

                // j'ai envoyer l'URL complet, je dois raccourcir pour juste avoir le filename
                var blob = _blobServiceClient.GetBlobContainerClient(_options.BlobContainer1).GetBlockBlobClient(Path.GetFileName(message!.Item1));

                MemoryStream ms = new MemoryStream();

                try
                {
                    await blob.DownloadToAsync(ms);
                    ms.Position = 0;

                    // https://docs.sixlabors.com/articles/imagesharp/resize.html
                    // If you pass 0 as any of the values for width and height dimensions then ImageSharp will automatically determine the correct opposite dimensions size to preserve the original aspect ratio.
                    using (Image image = Image.Load(ms))
                    {
                        image.Mutate(c => c.Resize(500, 0));
                        await image.SaveAsPngAsync(ms);
                        ms.Position = 0;
                    }

                    // Retourne l'image sur le second blob
                    await _blobServiceClient.GetBlobContainerClient(_options.BlobContainer2).UploadBlobAsync(Path.GetFileName(message!.Item1), ms);

                    // Destruction de l'image orignal
                    // await blob.DeleteAsync();

                    //Update Database
                    //Ajuster l'erreur management
                    try
                    {
                        ItemResponse<dynamic> response = await _container.ReadItemAsync<dynamic>(message!.Item2.ToString(), new Microsoft.Azure.Cosmos.PartitionKey());
                        var item = response.Resource;

                        // Update fields
                        item.Url = _blobServiceClient.Uri.AbsoluteUri + "/" + message!.Item1;

                        // Replace the item in the container
                        await _container.ReplaceItemAsync(item, message!.Item2.ToString(), new Microsoft.Azure.Cosmos.PartitionKey());

                        _logger.LogInformation($"Item with ID: {message!.Item2} updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating item.");
                    }

                    // Simulate work
                    await Task.Delay(5000);

                    // Complete the message
                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    // Si dans le traitement de l'image
                    await args.DeadLetterMessageAsync(args.Message);
                }
                finally
                {
                    ms.Dispose();
                }

                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message: {args.Message.MessageId}");
                await args.AbandonMessageAsync(args.Message);
            }
        }
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing messages.");
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }

            await _processor.StopProcessingAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.CloseAsync();
            await base.StopAsync(stoppingToken);
        }
    }
}

using MVC.Models;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using NuGet.Protocol;

namespace MVC.Business
{
    public class ServiceBusController
    {
        private ApplicationConfiguration _applicationConfiguration { get; }
        private ServiceBusClientOptions _serviceBusClientOptions { get; }

        public ServiceBusController(IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;

            // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
            // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
            // Service Bus Retry options
            // https://learn.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific

            _serviceBusClientOptions = new ServiceBusClientOptions
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
        }

        private async Task SendMessageAsync(string queueName, ServiceBusMessage message)
        {
            await using ServiceBusClient serviceBusClient = new ServiceBusClient(_applicationConfiguration.ServiceBusConnectionString, _serviceBusClientOptions);
            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);
            await serviceBusSender.SendMessageAsync(message);
        }

        public async Task SendImageToResize(string imageName, Guid Id)
        {
            Console.WriteLine("Envoi d'un message pour ImageResize : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new Tuple<string,Guid> (imageName,Id)));
            await SendMessageAsync(_applicationConfiguration.SB_resizeQueueName, message);
        }

        public async Task SendContentTextToValidation(string text, Guid Id)
        {
            Console.WriteLine("Envoi d'un message pour Text Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new ContentTypeValidation(ContentType.Text, text, Id)));
            await SendMessageAsync(_applicationConfiguration.SB_contentQueueName, message);
        }

        public async Task SendContentImageToValidation(string imageName, Guid Id)
        {
            Console.WriteLine("Envoi d'un message pour Image Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new ContentTypeValidation(ContentType.Image, imageName, Id)));
            await SendMessageAsync(_applicationConfiguration.SB_contentQueueName, message);
        }
    }

    [Serializable]
    public class ContentTypeValidation
    { 
        public ContentType ContentType { get; set; }
        public string Content { get; set; }
        public Guid ContentId { get; set; }

        public ContentTypeValidation(ContentType contentType, string content, Guid ContentId)
        {
            ContentType = contentType;
            Content = content;
            this.ContentId = ContentId;
        }   
    }

    public enum ContentType
    { 
        Image = 0,
        Text = 1,
    }
}

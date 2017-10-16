using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Infrastructure.Bus
{
    public static class ServiceBusClient
    {
        public static async Task SendEvent(object @event, string queue)
        {
            var serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");

            var client = QueueClient.CreateFromConnectionString(serviceBusEndpoint, queue);
            var message = new BrokeredMessage(@event);
            await client.SendAsync(message);
        }
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public class ServiceBusClient
    {
        private readonly string _serviceBusEndpoint;

        public ServiceBusClient()
        {
            _serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
        }

        public Task SendCommand<T>(T command)
            where T : IQueueBoundCommand
        {
            var queueName = command.QueueName;
            var queueClient = new QueueClient(_serviceBusEndpoint, queueName);
            var serialized = JsonConvert.SerializeObject(command);
            var message = new Message(Encoding.UTF8.GetBytes(serialized));
            return queueClient.SendAsync(message);
        }
    }
}
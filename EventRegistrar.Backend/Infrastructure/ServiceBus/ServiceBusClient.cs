using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public class ServiceBusClient
    {
        private readonly List<(string queueName, Message message)> _messages;
        private readonly string _serviceBusEndpoint;

        public ServiceBusClient()
        {
            _serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
            _messages = new List<(string queueName, Message message)>();
        }

        public async Task Release()
        {
            foreach (var queueMessages in _messages.GroupBy(msg => msg.queueName))
            {
                var queueName = queueMessages.Key;
                var queueClient = new QueueClient(_serviceBusEndpoint, queueName);
                foreach (var message in queueMessages.Select(msg => msg.message))
                {
                    await queueClient.SendAsync(message);
                }
            }
        }

        public void SendMessage<T>(T command)
            where T : IQueueBoundMessage
        {
            var serialized = JsonConvert.SerializeObject(command);
            var message = new Message(Encoding.UTF8.GetBytes(serialized));
            _messages.Add((command.QueueName, message));
        }
    }
}
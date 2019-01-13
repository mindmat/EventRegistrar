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
        public const string CommandQueueName = "CommandQueue";
        private readonly List<CommandMessage> _messages = new List<CommandMessage>();
        private readonly string _serviceBusEndpoint;

        public ServiceBusClient()
        {
            _serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
        }

        public async Task Release()
        {
            if (!_messages.Any())
            {
                return;
            }

            var queueClient = new QueueClient(_serviceBusEndpoint, CommandQueueName);
            foreach (var message in _messages)
            {
                var serialized = JsonConvert.SerializeObject(message);
                await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(serialized)));
            }
        }

        public void SendMessage<T>(T command)
        {
            var commandSerialized = JsonConvert.SerializeObject(command);
            _messages.Add(new CommandMessage { CommandType = command.GetType().FullName, CommandSerialized = commandSerialized });
        }
    }
}
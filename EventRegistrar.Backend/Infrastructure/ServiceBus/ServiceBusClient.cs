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

        //private readonly List<(string commandType, string commandSerialized)> _messages = new List<(string commandType, string commandSerialized)>();
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
            //foreach (var queueMessages in _messages.GroupBy(msg => msg.queueName))
            //{
            //    var queueName = queueMessages.Key;
            //    var queueClient = new QueueClient(_serviceBusEndpoint, queueName);
            //    foreach (var message in queueMessages.Select(msg => msg.message))
            //    {
            //        await queueClient.SendAsync(message);
            //    }
            //}
        }

        public void SendMessage<T>(T command)
        {
            var commandSerialized = JsonConvert.SerializeObject(command);
            //var message = new Message(Encoding.UTF8.GetBytes(serialized));
            _messages.Add(new CommandMessage { CommandType = command.GetType().FullName, CommandSerialized = commandSerialized });
        }
    }
}
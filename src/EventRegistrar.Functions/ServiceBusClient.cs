using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Functions
{
    public class ServiceBusClient
    {
        public static Task SendCommand(object command, string queueName = "CommandQueue")
        {
            var serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
            var queueClient = new QueueClient(serviceBusEndpoint, queueName);
            var serialized = JsonConvert.SerializeObject(command);
            var message = new Message(Encoding.UTF8.GetBytes(serialized));
            return queueClient.SendAsync(message);
        }
    }
}
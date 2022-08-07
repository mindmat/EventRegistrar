using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Functions
{
    public class ServiceBusClient
    {
        private const string _queueName = "CommandQueue";

        public static Task SendCommand(object command)
        {
            var serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
            var queueClient = new QueueClient(serviceBusEndpoint, _queueName);
            var serialized = JsonConvert.SerializeObject(command);
            var message = new Message(Encoding.UTF8.GetBytes(serialized));
            return queueClient.SendAsync(message);
        }
    }
}
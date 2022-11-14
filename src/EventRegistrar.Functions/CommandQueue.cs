using System;
using System.Text.Json;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

namespace EventRegistrar.Functions;

public static class CommandQueue
{
    private const string _queueName = "CommandQueue";

    public static async Task SendCommand(string commandTypeFullName, object? command = null)
    {
        var message = new
                      {
                          CommandType = commandTypeFullName,
                          CommandSerialized = command == null
                                                  ? "{}"
                                                  : JsonSerializer.Serialize(command)
                      };
        var serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint");
        var queueClient = new ServiceBusClient(serviceBusEndpoint);
        var sender = queueClient.CreateSender(_queueName);
        var serialized = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(serialized);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}
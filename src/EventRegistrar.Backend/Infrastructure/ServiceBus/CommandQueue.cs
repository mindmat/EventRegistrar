using System.Text;

using MediatR;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandQueue
{
    public const string CommandQueueName = "CommandQueue";
    private readonly List<CommandMessage> _messages = new();
    private readonly string _serviceBusEndpoint;

    public CommandQueue(IConfiguration configuration)
    {
        _serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint")
                           ?? configuration.GetValue<string>("ServiceBusEndpoint");
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

    public void EnqueueCommand<T>(T command)
        where T : IRequest
    {
        var commandSerialized = JsonConvert.SerializeObject(command);
        _messages.Add(new CommandMessage
                      {
                          CommandType = command.GetType().FullName!,
                          CommandSerialized = commandSerialized
                      });
    }
}
using Azure.Messaging.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandQueue
{
    private readonly ServiceBusSender _sender;
    public const string CommandQueueName = "CommandQueue";
    private readonly List<CommandMessage> _messages = new();

    public CommandQueue(ServiceBusSender sender)
    {
        _sender = sender;
    }

    public async Task Release()
    {
        if (!_messages.Any())
        {
            return;
        }

        await _sender.SendMessagesAsync(_messages.Select(msg => new ServiceBusMessage(JsonConvert.SerializeObject(msg))));
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
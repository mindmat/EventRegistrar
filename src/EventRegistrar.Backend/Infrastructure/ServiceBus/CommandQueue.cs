using Azure.Messaging.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandQueue(ServiceBusSender sender)
{
    public const string CommandQueueName = "CommandQueue";
    private readonly List<CommandMessage> _messages = new();

    public async Task Release()
    {
        if (!_messages.Any())
        {
            return;
        }

        foreach (var chunkOfMessages in _messages.Chunk(100))
        {
            await sender.SendMessagesAsync(chunkOfMessages.Select(msg => new ServiceBusMessage(JsonConvert.SerializeObject(msg))));
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
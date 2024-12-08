using Azure.Messaging.ServiceBus;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandQueue(ServiceBusSender sender)
{
    public const string CommandQueueName = "CommandQueue";
    private readonly List<(CommandMessage Command, bool SendAnyway)> _messages = [];

    public async Task Release(bool dbCommitSucceeded)
    {
        if (!_messages.Any())
        {
            return;
        }

        foreach (var chunkOfMessages in _messages.Where(msg=> dbCommitSucceeded || msg.SendAnyway)
                                                 .Chunk(100))
        {
            await sender.SendMessagesAsync(chunkOfMessages.Select(msg => new ServiceBusMessage(JsonConvert.SerializeObject(msg.Command))));
        }
    }

    public void EnqueueCommand<T>(T command, bool publishEvenWhenDbCommitFails = false)
        where T : IRequest
    {
        var commandSerialized = JsonConvert.SerializeObject(command);
        _messages.Add((new CommandMessage
                       {
                           CommandType = command.GetType().FullName!,
                           CommandSerialized = commandSerialized
                       }, publishEvenWhenDbCommitFails));
    }
}
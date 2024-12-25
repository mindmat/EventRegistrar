namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandMessage
{
    public string? CommandSerialized { get; init; }
    public string? CommandType { get; init; }
    public TimeSpan? Delay { get; init; }
}
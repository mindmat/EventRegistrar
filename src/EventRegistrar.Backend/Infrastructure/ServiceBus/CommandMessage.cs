namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class CommandMessage
{
    public string? CommandSerialized { get; set; }
    public string? CommandType { get; set; }
}
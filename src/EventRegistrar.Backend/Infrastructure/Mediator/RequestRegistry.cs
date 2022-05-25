namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class RequestRegistry
{
    public IEnumerable<Type> RequestTypes { get; }

    public RequestRegistry(IEnumerable<Type> requestTypes)
    {
        RequestTypes = requestTypes;
    }
}
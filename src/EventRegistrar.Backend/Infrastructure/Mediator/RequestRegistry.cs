namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class RequestRegistry
{
    public IEnumerable<(Type Request, Type RequestHandler)> RequestTypes { get; }

    public RequestRegistry(IEnumerable<Type> requestHandlerTypes)
    {
        RequestTypes = requestHandlerTypes.Select(rht => (rht.GetInterface(typeof(IRequestHandler<,>).Name)!.GetGenericArguments()[0], rht))
                                          .ToList();
    }
}
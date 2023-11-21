namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class RequestRegistry
{
    public IEnumerable<(Type Request, Type RequestHandler, RequestType Type)> RequestTypes { get; }

    public RequestRegistry(IEnumerable<Type> requestQueryTypes, IEnumerable<Type> requestCommandTypes)
    {
        RequestTypes = Enumerable.Concat(requestQueryTypes.Select(rht => (rht.GetInterface(typeof(IRequestHandler<,>).Name)!.GetGenericArguments()[0], rht, RequestType.Query)),
                                         requestCommandTypes.Select(rht => (rht.GetInterface(typeof(IRequestHandler<>).Name)!.GetGenericArguments()[0], rht, RequestType.Command)))
                                 .OrderBy(rht => rht.Item1.Name)
                                 .ToList();
    }
}

public enum RequestType
{
    Query = 1,
    Command = 2
}
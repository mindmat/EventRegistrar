using EventRegistrar.Backend.Infrastructure.ErrorHandling;

namespace EventRegistrar.Backend.Infrastructure;

public class FroalaKeyQuery : IRequest<string> { }

public class FroalaKeyQueryHandler(SecretReader secretReader) : IRequestHandler<FroalaKeyQuery, string>
{
    private const string Key = "FroalaKey";

    public async Task<string> Handle(FroalaKeyQuery query, CancellationToken cancellationToken)
    {
        return await secretReader.GetSecret(Key, cancellationToken) ?? throw new ConfigurationException(Key);
    }
}
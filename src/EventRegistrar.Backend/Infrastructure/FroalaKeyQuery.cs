using EventRegistrar.Backend.Infrastructure.ErrorHandling;

namespace EventRegistrar.Backend.Infrastructure;

public class FroalaKeyQuery : IRequest<string> { }

public class FroalaKeyQueryHandler : IRequestHandler<FroalaKeyQuery, string>
{
    private const string Key = "FroalaKey";
    private readonly SecretReader _secretReader;

    public FroalaKeyQueryHandler(SecretReader secretReader)
    {
        _secretReader = secretReader;
    }

    public async Task<string> Handle(FroalaKeyQuery query, CancellationToken cancellationToken)
    {
        return await _secretReader.GetSecret(Key, cancellationToken) ?? throw new ConfigurationException(Key);
    }
}
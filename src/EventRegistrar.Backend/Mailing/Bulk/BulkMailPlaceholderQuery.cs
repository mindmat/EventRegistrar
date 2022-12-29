using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailPlaceholderQuery : IRequest<IEnumerable<PlaceholderDescription>> { }

public class BulkMailPlaceholderQueryHandler : IRequestHandler<BulkMailPlaceholderQuery, IEnumerable<PlaceholderDescription>>
{
    private readonly EnumTranslator _enumTranslator;

    public BulkMailPlaceholderQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<PlaceholderDescription>> Handle(BulkMailPlaceholderQuery query, CancellationToken cancellationToken)
    {
        var values = Enum.GetValues<MailPlaceholder>()
                         .Select(mph => new PlaceholderDescription
                                        {
                                            Key = mph.ToString(),
                                            Placeholder = $"{{{{{mph}}}}}",
                                            Description = _enumTranslator.Translate(mph)
                                        });
        return Task.FromResult(values);
    }
}
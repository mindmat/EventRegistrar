using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailPlaceholderQuery : IRequest<IEnumerable<PlaceholderDescription>> { }

public class BulkMailPlaceholderQueryHandler(EnumTranslator enumTranslator) : IRequestHandler<BulkMailPlaceholderQuery, IEnumerable<PlaceholderDescription>>
{
    public Task<IEnumerable<PlaceholderDescription>> Handle(BulkMailPlaceholderQuery query, CancellationToken cancellationToken)
    {
        var values = Enum.GetValues<MailPlaceholder>()
                         .Select(mph => new PlaceholderDescription
                                        {
                                            Key = mph.ToString(),
                                            Placeholder = $"{{{{{mph}}}}}",
                                            Description = enumTranslator.Translate(mph)
                                        });
        return Task.FromResult(values);
    }
}
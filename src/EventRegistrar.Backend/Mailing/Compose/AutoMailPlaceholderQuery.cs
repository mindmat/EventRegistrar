using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Compose;

public class AutoMailPlaceholderQuery : IRequest<IEnumerable<PlaceholderDescription>> { }

public struct PlaceholderDescription
{
    public string Key { get; set; }
    public string Description { get; set; }
    public bool BothPartnerPossible { get; set; }
}

public class PartnerPlaceholderAttribute : Attribute { }

public enum MailPlaceholder
{
    [PartnerPlaceholder]
    FirstName = 1,

    [PartnerPlaceholder]
    LastName = 2
}

public class AutoMailPlaceholderQueryHandler : IRequestHandler<AutoMailPlaceholderQuery, IEnumerable<PlaceholderDescription>>
{
    private readonly EnumTranslator _enumTranslator;

    public AutoMailPlaceholderQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<PlaceholderDescription>> Handle(AutoMailPlaceholderQuery query, CancellationToken cancellationToken)
    {
        var values = Enum.GetValues<MailPlaceholder>()
                         .Select(mph => new PlaceholderDescription
                                        {
                                            Key = mph.ToString(),
                                            Description = _enumTranslator.Translate(mph),
                                            BothPartnerPossible = false
                                        });
        return Task.FromResult(values);
    }
}
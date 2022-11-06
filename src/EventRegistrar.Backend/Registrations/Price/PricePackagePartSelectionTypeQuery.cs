using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Pricing;

namespace EventRegistrar.Backend.Registrations.Price;

public class PricePackagePartSelectionTypeQuery : IRequest<IEnumerable<PricePackagePartSelectionTypeOption>>
{
    public Guid EventId { get; set; }
}

public class PricePackagePartSelectionTypeQueryHandler : IRequestHandler<PricePackagePartSelectionTypeQuery, IEnumerable<PricePackagePartSelectionTypeOption>>
{
    private readonly EnumTranslator _enumTranslator;

    public PricePackagePartSelectionTypeQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<PricePackagePartSelectionTypeOption>> Handle(PricePackagePartSelectionTypeQuery request,
                                                                         CancellationToken cancellationToken)
    {
        var mappings = _enumTranslator.TranslateAll<PricePackagePartSelectionType>()
                                      .Select(kvp => new PricePackagePartSelectionTypeOption
                                                     {
                                                         Type = kvp.Key,
                                                         Text = kvp.Value
                                                     });
        return Task.FromResult(mappings);
    }
}

public class PricePackagePartSelectionTypeOption
{
    public PricePackagePartSelectionType Type { get; set; }
    public string Text { get; set; } = null!;
}
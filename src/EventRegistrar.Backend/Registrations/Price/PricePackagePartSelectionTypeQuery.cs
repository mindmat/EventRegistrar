using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Pricing;

namespace EventRegistrar.Backend.Registrations.Price;

public class PricePackagePartSelectionTypeQuery : IRequest<IEnumerable<PricePackagePartSelectionTypeOption>>
{
    public Guid EventId { get; set; }
}

public class PricePackagePartSelectionTypeQueryHandler(EnumTranslator enumTranslator) : IRequestHandler<PricePackagePartSelectionTypeQuery, IEnumerable<PricePackagePartSelectionTypeOption>>
{
    public Task<IEnumerable<PricePackagePartSelectionTypeOption>> Handle(PricePackagePartSelectionTypeQuery query,
                                                                         CancellationToken cancellationToken)
    {
        var mappings = enumTranslator.TranslateAll<PricePackagePartSelectionType>()
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
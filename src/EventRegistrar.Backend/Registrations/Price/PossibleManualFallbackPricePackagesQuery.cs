namespace EventRegistrar.Backend.Registrations.Price;

public class PossibleManualFallbackPricePackagesQuery : IRequest<IEnumerable<FallbackPricePackage>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public record FallbackPricePackage(Guid Id, string Name);

public class PossibleManualFallbackPricePackagesQueryHandler : IRequestHandler<PossibleManualFallbackPricePackagesQuery, IEnumerable<FallbackPricePackage>>
{
    private readonly PriceCalculator _priceCalculator;

    public PossibleManualFallbackPricePackagesQueryHandler(PriceCalculator priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    public async Task<IEnumerable<FallbackPricePackage>> Handle(PossibleManualFallbackPricePackagesQuery query, CancellationToken cancellationToken)
    {
        var (_, _, _, _, _, _, possibleFallbackPackages) = await _priceCalculator.CalculatePrice(query.RegistrationId, cancellationToken);

        return possibleFallbackPackages.Where(ppk => ppk.Id != null)
                                       .Select(ppk => new FallbackPricePackage(ppk.Id!.Value, ppk.Name));
    }
}
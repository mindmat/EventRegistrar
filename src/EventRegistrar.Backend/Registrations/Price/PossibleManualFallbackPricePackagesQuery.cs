namespace EventRegistrar.Backend.Registrations.Price;

public class PossibleManualFallbackPricePackagesQuery : IRequest<IEnumerable<FallbackPricePackage>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public record FallbackPricePackage(Guid Id, string Name);

public class PossibleManualFallbackPricePackagesQueryHandler(PriceCalculator priceCalculator) : IRequestHandler<PossibleManualFallbackPricePackagesQuery, IEnumerable<FallbackPricePackage>>
{
    public async Task<IEnumerable<FallbackPricePackage>> Handle(PossibleManualFallbackPricePackagesQuery query, CancellationToken cancellationToken)
    {
        var calculatedPrice = await priceCalculator.CalculatePrice(query.RegistrationId, cancellationToken);

        return calculatedPrice.PossibleFallbackPackages
                              .Where(ppk => ppk.Id != null)
                              .Select(ppk => new FallbackPricePackage(ppk.Id!.Value, ppk.Name));
    }
}
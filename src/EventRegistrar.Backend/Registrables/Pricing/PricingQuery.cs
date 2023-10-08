namespace EventRegistrar.Backend.Registrables.Pricing;

public class PricingQuery : IRequest<IEnumerable<PricePackageDto>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PricingQueryHandler : IRequestHandler<PricingQuery, IEnumerable<PricePackageDto>>
{
    private readonly IQueryable<PricePackage> _packages;

    public PricingQueryHandler(IQueryable<PricePackage> packages)
    {
        _packages = packages;
    }

    public async Task<IEnumerable<PricePackageDto>> Handle(PricingQuery query, CancellationToken cancellationToken)
    {
        return await _packages.Where(ppg => ppg.EventId == query.EventId)
                              .OrderBy(ppg => ppg.SortKey)
                              .ThenBy(ppg => ppg.Name)
                              .Select(ppg => new PricePackageDto
                                             {
                                                 Id = ppg.Id,
                                                 Name = ppg.Name, Price = ppg.Price,
                                                 AllowAsAutomaticFallback = ppg.AllowAsAutomaticFallback,
                                                 AllowAsManualFallback = ppg.AllowAsManualFallback,
                                                 Parts = ppg.Parts!
                                                            .OrderBy(ppp => ppg.SortKey)
                                                            .Select(ppp => new PricePackagePartDto
                                                                           {
                                                                               Id = ppp.Id,
                                                                               SelectionType = ppp.SelectionType,
                                                                               PriceAdjustment = ppp.PriceAdjustment,
                                                                               RegistrableIds = ppp.Registrables!.Select(rip => rip.RegistrableId)
                                                                           })
                                             })
                              .ToListAsync(cancellationToken);
    }
}

public record PricePackageDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public bool AllowAsAutomaticFallback { get; set; }
    public bool AllowAsManualFallback { get; set; }
    public IEnumerable<PricePackagePartDto>? Parts { get; set; }
}

public record PricePackagePartDto
{
    public Guid Id { get; set; }
    public PricePackagePartSelectionType SelectionType { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public IEnumerable<Guid>? RegistrableIds { get; set; }
}
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
                              .Select(ppg => new PricePackageDto
                                             {
                                                 Id = ppg.Id,
                                                 Name = ppg.Name, Price = ppg.Price,
                                                 Parts = ppg.Parts!.Select(ppp => new PricePackagePartDto
                                                                                  {
                                                                                      Id = ppp.Id,
                                                                                      IsOptional = ppp.IsOptional,
                                                                                      Reduction = ppp.Reduction,
                                                                                      RegistrableIds = ppp.Registrables!.Select(rip => rip.RegistrableId)
                                                                                  })
                                             })
                              .OrderBy(ppg => ppg.Name)
                              .ToListAsync(cancellationToken);
    }
}

public record PricePackageDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public IEnumerable<PricePackagePartDto>? Parts { get; set; }
}

public record PricePackagePartDto
{
    public Guid Id { get; set; }
    public bool IsOptional { get; set; }
    public decimal? Reduction { get; set; }
    public IEnumerable<Guid>? RegistrableIds { get; set; }
}
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Pricing;

namespace EventRegistrar.Backend.Registrations.Price;

public class PricePackageOverviewQuery : IRequest<PricePackageOverview>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PricePackageOverview
{
    public IEnumerable<PricePackageCount> Packages { get; set; } = null!;
}

public class PricePackageCount
{
    public Guid PricePackageId { get; set; }
    public string Name { get; set; } = null!;
    public int Count { get; set; }
}

public class PricePackageOverviewQueryHandler : IRequestHandler<PricePackageOverviewQuery, PricePackageOverview>
{
    private readonly IQueryable<PricePackage> _pricePackages;
    private readonly IQueryable<Registration> _registrations;

    public PricePackageOverviewQueryHandler(IQueryable<PricePackage> pricePackages,
                                            IQueryable<Registration> registrations)
    {
        _pricePackages = pricePackages;
        _registrations = registrations;
    }

    public async Task<PricePackageOverview> Handle(PricePackageOverviewQuery query, CancellationToken cancellationToken)
    {
        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                           && reg.PricePackageIds_Admitted != null)
                                                .Select(reg => new
                                                               {
                                                                   reg.Id,
                                                                   PricePackageIds_Admitted = reg.PricePackageIds_Admitted.SplitGuidKeys()
                                                               })
                                                .ToListAsync(cancellationToken);
        var packagesCounts = registrations.SelectMany(reg => reg.PricePackageIds_Admitted)
                                          .GroupBy(grp => grp)
                                          .ToDictionary(grp => grp.Key, grp => grp.Count());

        var packages = await _pricePackages.Where(pkg => pkg.EventId == query.EventId
                                                      && packagesCounts.Keys.Contains(pkg.Id))
                                           .ToDictionaryAsync(pkg => pkg.Id, pkg => pkg.Name, cancellationToken);

        return new PricePackageOverview
               {
                   Packages = packagesCounts.Select(pkg => new PricePackageCount
                                                           {
                                                               PricePackageId = pkg.Key,
                                                               Name = packages[pkg.Key],
                                                               Count = pkg.Value
                                                           })
               };
    }
}
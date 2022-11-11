using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceCalculator
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Seat> _spots;
    private readonly IQueryable<PricePackage> _pricePackages;

    public PriceCalculator(IQueryable<Seat> spots,
                           IQueryable<PricePackage> pricePackages,
                           IQueryable<Registration> registrations)
    {
        _spots = spots;
        _pricePackages = pricePackages;
        _registrations = registrations;
    }

    public async Task<decimal> CalculatePrice(Guid registrationId)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == registrationId);
        var spots = await _spots.Where(spot => spot.RegistrationId == registrationId
                                            || spot.RegistrationId_Follower == registrationId)
                                .Where(spot => !spot.IsCancelled)
                                .Include(spot => spot.Registrable)
                                .ToListAsync();

        return await CalculatePrice(registration, spots);
    }

    public async Task<decimal> CalculatePrice(Registration registration, IEnumerable<Seat> spots)
    {
        var notCancelledSpots = spots.Where(spot => !spot.IsCancelled
                                                 && (spot.RegistrationId == registration.Id
                                                  || spot.RegistrationId_Follower == registration.Id))
                                     .ToList();

        var bookedRegistrableIds = new HashSet<Guid>(notCancelledSpots.Select(seat => seat.RegistrableId));

        var packages = await _pricePackages.Where(ppg => ppg.EventId == registration.EventId)
                                           .Include(ppg => ppg.Parts!)
                                           .ThenInclude(ppp => ppp.Registrables!)
                                           .ToListAsync();
        List<(PricePackage Package, IReadOnlyCollection<Guid> MatchingRequiredRegistrableId, IReadOnlyCollection<Guid> MatchingOptionalRegistrableId, decimal Price)> matchingPackages = new();
        foreach (var package in packages)
        {
            var packageMatches = true;
            var packagePrice = package.Price;
            var matchingRequiredRegistrableIds = new HashSet<Guid>();
            var matchingOptionalRegistrableIds = new HashSet<Guid>();
            foreach (var part in package.Parts!)
            {
                var partMatches = PartMatches(part.SelectionType, part.Registrables!.Select(rip => rip.RegistrableId).ToList(), bookedRegistrableIds);
                if (partMatches.Match)
                {
                    matchingRequiredRegistrableIds.AddRange(partMatches.MatchingRequiredRegistrableIds);
                    matchingOptionalRegistrableIds.AddRange(partMatches.MatchingOptionalRegistrableIds);
                    if (part.PriceAdjustment != null)
                    {
                        packagePrice += part.PriceAdjustment.Value;
                    }
                }
                else
                {
                    if (part.SelectionType != PricePackagePartSelectionType.Optional)
                    {
                        packageMatches = false;
                    }
                }
            }

            if (packageMatches && (matchingRequiredRegistrableIds.Any() || matchingOptionalRegistrableIds.Any()))
            {
                matchingPackages.Add(
                    new ValueTuple<PricePackage, IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>, decimal>(package, matchingRequiredRegistrableIds, matchingOptionalRegistrableIds, packagePrice));
            }
        }

        var overlappingRegistrableIds = matchingPackages.SelectMany(ppk => ppk.MatchingRequiredRegistrableId.Concat(ppk.MatchingOptionalRegistrableId))
                                                        .GroupBy(ppk => ppk)
                                                        .Where(rid => rid.Count() > 1)
                                                        .ToDictionary(ppk => ppk.Key, ppk => ppk.Count());
        if (overlappingRegistrableIds.Any())
        {
            var coveredRegistrableIds = new HashSet<Guid>();
            foreach (var matchingPackage in matchingPackages.OrderByDescending(ppk => ppk.MatchingRequiredRegistrableId.Count
                                                                                    + ppk.MatchingOptionalRegistrableId.Count)
                                                            .ToList())
            {
                if (matchingPackage.MatchingRequiredRegistrableId.All(mri => coveredRegistrableIds.Contains(mri)))
                {
                    matchingPackages.Remove(matchingPackage);
                }
                else
                {
                    coveredRegistrableIds.AddRange(matchingPackage.MatchingRequiredRegistrableId);
                    coveredRegistrableIds.AddRange(matchingPackage.MatchingOptionalRegistrableId);
                }
            }
        }

        var price = matchingPackages.Sum(ppk => ppk.Price);
        return price;
    }

    private static (bool Match,
        IEnumerable<Guid> MatchingRequiredRegistrableIds,
        IEnumerable<Guid> MatchingOptionalRegistrableIds)
        PartMatches(PricePackagePartSelectionType selectionType,
                    IReadOnlyCollection<Guid> partRegistrableIds,
                    IEnumerable<Guid> bookedRegistrableIds)
    {
        var matchingRegistrableIds = bookedRegistrableIds.Where(partRegistrableIds.Contains)
                                                         .ToList();
        var match = selectionType switch
        {
            PricePackagePartSelectionType.All      => matchingRegistrableIds.Count == partRegistrableIds.Count,
            PricePackagePartSelectionType.AnyOne   => matchingRegistrableIds.Count == 1,
            PricePackagePartSelectionType.AnyTwo   => matchingRegistrableIds.Count == 2,
            PricePackagePartSelectionType.AnyThree => matchingRegistrableIds.Count == 3,
            PricePackagePartSelectionType.Optional => matchingRegistrableIds.Count > 0,
            _                                      => false
        };
        return (match,
                   selectionType != PricePackagePartSelectionType.Optional ? matchingRegistrableIds : Enumerable.Empty<Guid>(),
                   selectionType == PricePackagePartSelectionType.Optional ? matchingRegistrableIds : Enumerable.Empty<Guid>());
    }
}
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceCalculator
{
    private readonly IQueryable<Registration> _registrations;
    private readonly EnumTranslator _enumTranslator;
    private readonly IQueryable<Seat> _spots;
    private readonly IQueryable<PricePackage> _pricePackages;

    public PriceCalculator(IQueryable<Seat> spots,
                           IQueryable<PricePackage> pricePackages,
                           IQueryable<Registration> registrations,
                           EnumTranslator enumTranslator)
    {
        _spots = spots;
        _pricePackages = pricePackages;
        _registrations = registrations;
        _enumTranslator = enumTranslator;
    }

    public async Task<(decimal priceOriginal, decimal priceAdmitted, decimal priceAdmittedAndReduced, IReadOnlyCollection<MatchingPackageResult> packagesOriginal,
        IReadOnlyCollection<MatchingPackageResult>
        packagesAdmitted)> CalculatePrice(Guid registrationId, CancellationToken cancellationToken = default)
    {
        var registration = await _registrations.Where(reg => reg.Id == registrationId)
                                               .Include(reg => reg.IndividualReductions)
                                               .FirstAsync(cancellationToken);
        var spots = await _spots.Where(spot => spot.RegistrationId == registrationId
                                            || spot.RegistrationId_Follower == registrationId)
                                .Where(spot => !spot.IsCancelled)
                                .Include(spot => spot.Registrable)
                                .ToListAsync(cancellationToken);

        return await CalculatePrice(registration, spots);
    }

    public async Task<(decimal priceOriginal, decimal priceAdmitted, decimal priceAdmittedAndReduced, IReadOnlyCollection<MatchingPackageResult> packagesOriginal,
        IReadOnlyCollection<MatchingPackageResult>
        packagesAdmitted)> CalculatePrice(Registration registration,
                                          IEnumerable<Seat> spots)
    {
        var notCancelledSpots = spots.Where(spot => !spot.IsCancelled
                                                 && (spot.RegistrationId == registration.Id
                                                  || spot.RegistrationId_Follower == registration.Id))
                                     .ToList();

        var packages = await _pricePackages.Where(ppg => ppg.EventId == registration.EventId)
                                           .Include(ppg => ppg.Parts!)
                                           .ThenInclude(ppp => ppp.Registrables!)
                                           .ThenInclude(rip => rip.Registrable)
                                           .ToListAsync();
        var (priceOriginal, packagesOriginal) = CalculatePriceOfSpots(registration.Id, notCancelledSpots, packages);

        var admittedSpots = notCancelledSpots.Where(spot => !spot.IsWaitingList)
                                             .ToList();
        var (priceAdmitted, packagesAdmitted) = CalculatePriceOfSpots(registration.Id, admittedSpots, packages);

        var (priceAdmittedAndReduced, reductionPackage) = GetReducedPrice(priceAdmitted, registration.IndividualReductions);
        if (reductionPackage != null)
        {
            packagesAdmitted = packagesAdmitted.Append(reductionPackage.Value).ToList();
        }

        return (priceOriginal, priceAdmitted, priceAdmittedAndReduced, packagesOriginal, packagesAdmitted);
    }

    private static (decimal Price, MatchingPackageResult? ReductionPackage) GetReducedPrice(decimal priceNotReduced, ICollection<IndividualReduction>? individualReductions)
    {
        if (individualReductions?.Any() != true)
        {
            return (priceNotReduced, null);
        }

        var overwrite = individualReductions.Where(idr => idr.Type == IndividualReductionType.OverwritePrice)
                                            .MinBy(idr => idr.Amount);
        if (overwrite != null)
        {
            var reducedPrice = Math.Max(priceNotReduced, overwrite.Amount);
            return (overwrite.Amount, new MatchingPackageResult(overwrite.Reason ?? Resources.Reduction,
                                                                reducedPrice - priceNotReduced,
                                                                new List<MatchingPackageSpot>()));
        }

        var totalReduction = individualReductions.Select(ird => ird.Amount)
                                                 .Sum();
        var priceAdmittedAndReduced = Math.Max(0m, priceNotReduced - totalReduction);
        return (priceAdmittedAndReduced, new MatchingPackageResult(Resources.Reduction,
                                                                   -totalReduction,
                                                                   individualReductions.Select(ird => new MatchingPackageSpot(ird.Reason ?? Resources.Reduction, -ird.Amount))));
    }

    private (decimal Price, IReadOnlyCollection<MatchingPackageResult> matchingPackages) CalculatePriceOfSpots(Guid registrationId, IReadOnlyCollection<Seat> spots, List<PricePackage> packages)
    {
        var bookedRegistrableIds = new HashSet<Guid>(spots.Select(spot => spot.RegistrableId));
        List<MatchingPackage> matchingPackages = new();
        foreach (var package in packages)
        {
            var packageMatches = true;
            var packagePrice = package.Price;
            var matchingRequiredRegistrableIds = new HashSet<Guid>();
            var matchingOptionalRegistrableIds = new HashSet<Guid>();
            var matchingSpots = new List<MatchingPackageSpot>();
            foreach (var part in package.Parts!)
            {
                var partMatches = PartMatches(part.SelectionType,
                                              part.Registrables!
                                                  .Select(rip => rip.RegistrableId)
                                                  .ToList(),
                                              bookedRegistrableIds);
                if (partMatches.Match)
                {
                    matchingRequiredRegistrableIds.AddRange(partMatches.MatchingRequiredRegistrableIds);
                    matchingOptionalRegistrableIds.AddRange(partMatches.MatchingOptionalRegistrableIds);
                    var matchingSpotsOfPart = partMatches.MatchingRequiredRegistrableIds.Select(mtc => new MatchingPackageSpot(GetRegistrableName(registrationId, mtc, part.Registrables!, spots)))
                                                         .Concat(partMatches.MatchingOptionalRegistrableIds.Select(
                                                                     mtc => new MatchingPackageSpot(GetRegistrableName(registrationId, mtc, part.Registrables!, spots))))
                                                         .ToList();
                    if (part.PriceAdjustment != null)
                    {
                        packagePrice += part.PriceAdjustment.Value;
                        matchingSpotsOfPart.First().PriceAdjustment = part.PriceAdjustment.Value;
                    }

                    matchingSpots.AddRange(matchingSpotsOfPart);
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
                matchingPackages.Add(new MatchingPackage(package, matchingRequiredRegistrableIds, matchingOptionalRegistrableIds, packagePrice, matchingSpots));
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
        return (price, matchingPackages.Select(pkg => new MatchingPackageResult
                                                      {
                                                          Name = pkg.Package.Name,
                                                          Price = pkg.Price,
                                                          Spots = pkg.Spots
                                                      })
                                       .ToList());
    }

    private string GetRegistrableName(Guid registrationId,
                                      Guid registrableId,
                                      IEnumerable<RegistrableInPricePackagePart> registrableInPricePackageParts,
                                      IReadOnlyCollection<Seat> spots)
    {
        var registrable = registrableInPricePackageParts.First(rip => rip.RegistrableId == registrableId);
        if (registrable.Registrable!.Type == RegistrableType.Double)
        {
            var spot = spots.First(spt => spt.RegistrableId == registrableId);
            var role = spot.RegistrationId_Follower == registrationId
                           ? Role.Follower
                           : Role.Leader;
            return $"{registrable.Registrable.DisplayName} ({_enumTranslator.Translate(role)})";
        }

        return registrable.Registrable.DisplayName;
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

public record struct MatchingPackage(PricePackage Package,
                                     IReadOnlyCollection<Guid> MatchingRequiredRegistrableId,
                                     IReadOnlyCollection<Guid> MatchingOptionalRegistrableId,
                                     decimal Price,
                                     IEnumerable<MatchingPackageSpot> Spots);

public record struct MatchingPackageResult(string Name,
                                           decimal Price,
                                           IEnumerable<MatchingPackageSpot> Spots);

public record MatchingPackageSpot(string Name,
                                  decimal? PriceAdjustment = null)
{
    public decimal? PriceAdjustment { get; set; } = PriceAdjustment;
}
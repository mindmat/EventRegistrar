using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceCalculator(IQueryable<Seat> _spots,
                             IQueryable<PricePackage> pricePackages,
                             IQueryable<Registration> registrations,
                             IQueryable<Registrable> tracks,
                             EnumTranslator enumTranslator)
{
    public async Task<(decimal priceOriginal,
            decimal priceAdmitted,
            decimal priceAdmittedAndReduced,
            IReadOnlyCollection<MatchingPackageResult> packagesRequested,
            IReadOnlyCollection<MatchingPackageResult> packagesAdmitted,
            bool isOnWaitingList,
            IEnumerable<MatchingPackageResult> possibleFallbackPackages)>
        CalculatePrice(Guid registrationId, CancellationToken cancellationToken = default)
    {
        var registration = await registrations.Where(reg => reg.Id == registrationId)
                                              .Include(reg => reg.IndividualReductions)
                                              .FirstAsync(cancellationToken);
        var spots = await _spots.Where(spot => spot.RegistrationId == registrationId
                                            || spot.RegistrationId_Follower == registrationId)
                                .Where(spot => !spot.IsCancelled)
                                .Include(spot => spot.Registrable)
                                .ToListAsync(cancellationToken);

        return await CalculatePrice(registration, spots);
    }

    public async Task<(decimal priceOriginal,
            decimal priceAdmitted,
            decimal priceAdmittedAndReduced,
            IReadOnlyCollection<MatchingPackageResult> packagesRequested,
            IReadOnlyCollection<MatchingPackageResult> packagesAdmitted,
            bool isOnWaitingList,
            IEnumerable<MatchingPackageResult> possibleFallbackPackages)>
        CalculatePrice(Registration registration,
                       IEnumerable<Seat> spots)
    {
        var coreTracks = await tracks.Where(trk => trk.EventId == registration.EventId && trk.IsCore)
                                     .ToListAsync();
        var isOnWaitingList = false;
        var notCancelledSpots = spots.Where(spot => !spot.IsCancelled
                                                 && (spot.RegistrationId == registration.Id
                                                  || spot.RegistrationId_Follower == registration.Id))
                                     .ToList();

        var packages = await pricePackages.Where(ppg => ppg.EventId == registration.EventId)
                                          .Include(ppg => ppg.Parts!.OrderBy(ppp => ppp.SortKey))
                                          .ThenInclude(ppp => ppp.Registrables!)
                                          .ThenInclude(rip => rip.Registrable)
                                          .OrderBy(ppg => ppg.SortKey)
                                          .ToListAsync();
        var (priceOriginal, packagesOriginal, allCoveredOriginal) = CalculatePriceOfSpots(registration.Id, notCancelledSpots, packages, coreTracks);

        var hasSpotsOnWaitingList = notCancelledSpots.Exists(spot => spot.IsWaitingList);
        var priceAdmitted = priceOriginal;
        var packagesAdmitted = packagesOriginal;
        var originalPackageIds = packagesOriginal.Select(pkg => pkg.Id).ToList();
        var possibleFallbackPackages = Enumerable.Empty<MatchingPackageResult>();

        if (hasSpotsOnWaitingList || !allCoveredOriginal)
        {
            isOnWaitingList = true;
            var admittedSpots = notCancelledSpots.Where(spot => !spot.IsWaitingList)
                                                 .ToList();
            (priceAdmitted, packagesAdmitted, var allCoveredAdmitted) = CalculatePriceOfSpots(registration.Id, admittedSpots, packages, coreTracks);
            var admittedPackagesId = packagesAdmitted.Select(pkg => pkg.Id).ToList();

            var samePackages = admittedPackagesId.All(originalPackageIds.Contains);
            if (!samePackages)
            {
                var fallbackPackages = packagesAdmitted.Where(adm => !originalPackageIds.Contains(adm.Id))
                                                       .ToList();
                if (fallbackPackages.All(ppk => ppk.AllowAsAutomaticFallback
                                             || (ppk.AllowAsManualFallback && ppk.Id == registration.PricePackageId_ManualFallback)))
                {
                    // allow fallback
                    isOnWaitingList = !allCoveredAdmitted;
                }
                else
                {
                    // don't allow fallback
                    priceAdmitted = 0m;
                    packagesAdmitted = new List<MatchingPackageResult>(0);
                    isOnWaitingList = true;
                }

                possibleFallbackPackages = fallbackPackages.Where(ppk => ppk.AllowAsManualFallback);
            }
        }

        var (priceAdmittedAndReduced, reductionPackage) = GetReducedPrice(priceAdmitted, registration.IndividualReductions);
        if (reductionPackage != null)
        {
            packagesAdmitted = packagesAdmitted.Append(reductionPackage.Value).ToList();
        }

        return (priceOriginal, priceAdmitted, priceAdmittedAndReduced, packagesOriginal, packagesAdmitted, isOnWaitingList, possibleFallbackPackages);
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
            // set new price
            var reducedPrice = Math.Clamp(overwrite.Amount, 0, priceNotReduced);
            return (reducedPrice, new MatchingPackageResult(null,
                                                            $"{Resources.Reduction}: {overwrite.Reason}",
                                                            reducedPrice - priceNotReduced,
                                                            priceNotReduced,
                                                            false,
                                                            false,
                                                            Array.Empty<MatchingPackageSpot>()));
        }

        // adjust price
        var totalReduction = individualReductions.Select(ird => ird.Amount)
                                                 .Sum();
        var clampedReduction = Math.Clamp(totalReduction, 0, priceNotReduced);
        var priceAdmittedAndReduced = priceNotReduced - clampedReduction;
        return (priceAdmittedAndReduced, new MatchingPackageResult(null,
                                                                   Resources.Reduction,
                                                                   0m,
                                                                   0m,
                                                                   false,
                                                                   false,
                                                                   individualReductions.Select(ird => new MatchingPackageSpot(ird.Reason ?? Resources.Reduction, -ird.Amount)),
                                                                   true));
    }

    public (decimal Price, IReadOnlyCollection<MatchingPackageResult> matchingPackages, bool allSpotsCovered) CalculatePriceOfSpots(Guid registrationId,
                                                                                                                                    IReadOnlyCollection<Seat> spots,
                                                                                                                                    IEnumerable<PricePackage> packages,
                                                                                                                                    IReadOnlyCollection<Registrable> coreTracks)
    {
        var bookedRegistrableIds = new HashSet<Guid>(spots.Select(spot => spot.RegistrableId));
        var bookedCoreRegistrableIds = new HashSet<Guid>(spots.Select(spot => spot.RegistrableId).Where(rid => coreTracks.Select(trk => trk.Id).Contains(rid)));
        var matchingPackages = new List<MatchingPackage>();
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
                                              part.Registrables!.Select(rip => rip.RegistrableId)
                                                  .ToList(),
                                              bookedRegistrableIds);
                if (partMatches.Match)
                {
                    matchingRequiredRegistrableIds.AddRange(partMatches.MatchingRequiredRegistrableIds);
                    matchingOptionalRegistrableIds.AddRange(partMatches.MatchingOptionalRegistrableIds);
                    var matchingSpotsOfPart = partMatches.MatchingRequiredRegistrableIds.Select(mtc =>
                                                         {
                                                             var (name, sortKey) = GetRegistrable(registrationId, mtc, part.Registrables!, spots);
                                                             return new MatchingPackageSpot(name, null, part.ShowInMailSpotList ? sortKey : null);
                                                         })
                                                         .Concat(partMatches.MatchingOptionalRegistrableIds.Select(mtc =>
                                                         {
                                                             var (name, sortKey) = GetRegistrable(registrationId, mtc, part.Registrables!, spots);
                                                             return new MatchingPackageSpot(name, null, part.ShowInMailSpotList ? sortKey : null);
                                                         }))
                                                         .ToList();
                    if (part is { PriceAdjustment: not null, SelectionType: PricePackagePartSelectionType.Optional })
                    {
                        packagePrice += part.PriceAdjustment.Value;
                        matchingSpotsOfPart[0].PriceAdjustment = part.PriceAdjustment.Value;
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
                matchingPackages.Add(new MatchingPackage(package,
                                                         matchingRequiredRegistrableIds,
                                                         matchingOptionalRegistrableIds,
                                                         packagePrice,
                                                         package.Price,
                                                         matchingSpots));
            }
        }

        var overlappingRegistrableIds = matchingPackages.SelectMany(ppk => ppk.MatchingRequiredRegistrableId.Concat(ppk.MatchingOptionalRegistrableId))
                                                        .GroupBy(ppk => ppk)
                                                        .Where(rid => rid.Count() > 1)
                                                        .ToDictionary(ppk => ppk.Key, ppk => ppk.Count());
        if (overlappingRegistrableIds.Any())
        {
            var coveredRegistrableIds = new HashSet<Guid>();
            foreach (var matchingPackage in matchingPackages.OrderBy(ppk => ppk.Package.FallbackPriority)
                                                            .ToList())
            {
                if (matchingPackage.MatchingRequiredRegistrableId.All(coveredRegistrableIds.Contains))
                {
                    // all tracks are covered by other packages
                    matchingPackages.Remove(matchingPackage);
                }
                else
                {
                    coveredRegistrableIds.AddRange(matchingPackage.MatchingRequiredRegistrableId);
                    coveredRegistrableIds.AddRange(matchingPackage.MatchingOptionalRegistrableId);
                }
            }
        }

        var notCoveredRegistrableIds = bookedCoreRegistrableIds.Except(matchingPackages.SelectMany(pkg => pkg.MatchingRequiredRegistrableId.Concat(pkg.MatchingOptionalRegistrableId)));
        var price = matchingPackages.Sum(ppk => ppk.Price);
        return (price,
                   matchingPackages.Select(pkg => new MatchingPackageResult
                                           (
                                               pkg.Package.Id,
                                               pkg.Package.Name,
                                               pkg.Price,
                                               pkg.OriginalPrice,
                                               pkg.Package.AllowAsAutomaticFallback,
                                               pkg.Package.AllowAsManualFallback,
                                               pkg.Spots
                                           ))
                                   .ToList(),
                   !notCoveredRegistrableIds.Any());
    }

    private (string Name, int? SortKey) GetRegistrable(Guid registrationId,
                                                       Guid registrableId,
                                                       IEnumerable<RegistrableInPricePackagePart> registrableInPricePackageParts,
                                                       IEnumerable<Seat> spots)
    {
        var registrable = registrableInPricePackageParts.First(rip => rip.RegistrableId == registrableId);
        if (registrable.Registrable!.Type == RegistrableType.Double)
        {
            var spot = spots.First(spt => spt.RegistrableId == registrableId);
            var role = spot.RegistrationId_Follower == registrationId
                           ? Role.Follower
                           : Role.Leader;
            return ($"{registrable.Registrable.DisplayName} ({enumTranslator.Translate(role)})", registrable.Registrable.ShowInMailListOrder);
        }

        return (registrable.Registrable.DisplayName, registrable.Registrable.ShowInMailListOrder);
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
                                     decimal OriginalPrice,
                                     IEnumerable<MatchingPackageSpot> Spots);

public record struct MatchingPackageResult(Guid? Id,
                                           string Name,
                                           decimal Price,
                                           decimal OriginalPrice,
                                           bool AllowAsAutomaticFallback,
                                           bool AllowAsManualFallback,
                                           IEnumerable<MatchingPackageSpot> Spots,
                                           bool IsReductionsPackage = false);

public record MatchingPackageSpot(string Name,
                                  decimal? PriceAdjustment = null,
                                  int? SortKey = int.MaxValue)
{
    public decimal? PriceAdjustment { get; set; } = PriceAdjustment;
}
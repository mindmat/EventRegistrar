using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public record CalculatedPrice(decimal PriceOriginal,
                              decimal PriceAdmitted,
                              decimal PriceAdmittedAndReduced,
                              IReadOnlyCollection<MatchingPackageResult> PackagesRequested,
                              IReadOnlyCollection<MatchingPackageResult> PackagesAdmitted,
                              bool IsOnWaitingList,
                              IEnumerable<MatchingPackageResult> PossibleFallbackPackages,
                              IDictionary<Guid, string> SpotsOnWaitingList);

public class PriceCalculator(IQueryable<Seat> _spots,
                             IQueryable<PricePackage> pricePackages,
                             IQueryable<Registration> registrations,
                             IQueryable<Registrable> tracks,
                             EnumTranslator enumTranslator)
{
    public async Task<CalculatedPrice> CalculatePrice(Guid registrationId, CancellationToken cancellationToken = default)
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

    public async Task<CalculatedPrice> CalculatePrice(Registration registration,
                                                      IEnumerable<Seat> spots)
    {
        var allTracks = await tracks.Where(trk => trk.EventId == registration.EventId)
                                    .ToListAsync();
        var coreTracks = allTracks.Where(trk => trk.IsCore)
                                  .ToList();
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

        var spotsOnWaitingList = notCancelledSpots.Where(spot => spot.IsWaitingList)
                                                  .ToList();
        var priceAdmitted = priceOriginal;
        var packagesAdmitted = packagesOriginal;
        var originalPackageIds = packagesOriginal.Select(pkg => pkg.Id).ToList();
        var possibleFallbackPackages = Enumerable.Empty<MatchingPackageResult>();

        if (spotsOnWaitingList.Any() || !allCoveredOriginal)
        {
            var admittedSpots = notCancelledSpots.Where(spot => !spot.IsWaitingList)
                                                 .ToList();
            (priceAdmitted, packagesAdmitted, var allCoveredAdmitted) = CalculatePriceOfSpots(registration.Id, admittedSpots, packages, coreTracks);
            var admittedPackagesId = packagesAdmitted.Select(pkg => pkg.Id).ToList();

            var samePackages = Enumerable.SequenceEqual(admittedPackagesId.OrderBy(id => id),
                                                        originalPackageIds.OrderBy(id => id));
            if (!samePackages)
            {
                var fallbackPackages = packagesAdmitted.Where(adm => !originalPackageIds.Contains(adm.Id))
                                                       .ToList();
                var allPossibleAsFallback = fallbackPackages.All(ppk => ppk is { AllowAsAutomaticFallback: true }
                                                                     || (ppk is { AllowAsManualFallback: true, Id: not null }
                                                                      && registration.PricePackageIds_ManualFallback?.Contains(ppk.Id.Value) == true));
                var combinationOffWaitingList = fallbackPackages.Any(ppk => ppk.IsCorePackage
                                                                         || ppk is { AllowAsManualFallback: true, Id: not null }
                                                                         && registration.PricePackageIds_ManualFallback?.Contains(ppk.Id.Value) == true);
                if (allPossibleAsFallback && combinationOffWaitingList)
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

                possibleFallbackPackages = Enumerable.Concat(fallbackPackages.Where(ppk => ppk.AllowAsManualFallback),
                                                             packagesAdmitted.Where(ppk => !ppk.IsCorePackage))
                                                     .DistinctBy(ppk => ppk.Id);
            }
            else
            {
                var anyCorePackageAdmitted = packagesAdmitted.Any(ppk => ppk.IsCorePackage);
                isOnWaitingList = !anyCorePackageAdmitted;
            }
        }

        decimal priceAdmittedAndReduced;
        if (isOnWaitingList)
        {
            priceAdmitted = 0m;
            priceAdmittedAndReduced = 0m;
            packagesAdmitted = [];
            spotsOnWaitingList = notCancelledSpots.Where(spot => allTracks.Any(trk => trk.Id == spot.RegistrableId
                                                                                   && trk.HasWaitingList))
                                                  .ToList();
        }
        else
        {
            (priceAdmittedAndReduced, var reductionPackage) = GetReducedPrice(priceAdmitted, registration.IndividualReductions);
            if (reductionPackage != null)
            {
                packagesAdmitted = packagesAdmitted.Append(reductionPackage.Value).ToList();
            }
        }

        return new CalculatedPrice(priceOriginal,
                                   priceAdmitted,
                                   priceAdmittedAndReduced,
                                   packagesOriginal,
                                   packagesAdmitted,
                                   isOnWaitingList,
                                   possibleFallbackPackages,
                                   spotsOnWaitingList.ToDictionary(spt => spt.RegistrableId,
                                                                   spt => spt.Registrable?.DisplayName ?? spt.RegistrableId.ToString()));
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
                                                            0m,
                                                            false,
                                                            false,
                                                            false,
                                                            Array.Empty<MatchingPackageSpot>(),
                                                            true));
        }

        // adjust price
        var price = priceNotReduced;
        var totalReductionFactor = individualReductions.Where(ird => ird.Type == IndividualReductionType.Percentage)
                                                       .Select(ird => (decimal?)ird.Amount)
                                                       .DefaultIfEmpty()
                                                       .Sum();
        if (totalReductionFactor != null)
        {
            totalReductionFactor = Math.Clamp(totalReductionFactor.Value, 0, 1);
            price *= 1 - totalReductionFactor.Value;
        }

        var totalReductionAmount = individualReductions.Where(ird => ird.Type == IndividualReductionType.Reduction)
                                                       .Select(ird => (decimal?)ird.Amount)
                                                       .DefaultIfEmpty()
                                                       .Sum();
        if (totalReductionAmount != null)
        {
            var clampedReduction = Math.Clamp(totalReductionAmount.Value, 0, price);
            price -= clampedReduction;
        }

        var reductionText = Resources.Reduction;
        var reductionReasons = individualReductions.Select(red => red.Reason).StringJoinNullable();
        if (reductionReasons != null)
        {
            reductionText = $"{Resources.Reduction}: {reductionReasons}";
        }

        var totalReduction = priceNotReduced - price;
        return (price, new MatchingPackageResult(null,
                                                 reductionText,
                                                 -totalReduction,
                                                 0m,
                                                 false,
                                                 false,
                                                 false,
                                                 individualReductions.Select(reduction => GetReductionLine(reduction, priceNotReduced))
                                                                     .WhereNotNull(),
                                                 true));
    }

    private static MatchingPackageSpot? GetReductionLine(IndividualReduction reduction, decimal originalPrice)
    {
        switch (reduction.Type)
        {
            case IndividualReductionType.Reduction:
                return new MatchingPackageSpot(reduction.Reason ?? Resources.Reduction, -reduction.Amount);
            case IndividualReductionType.Percentage:
                {
                    var factor = Math.Clamp(reduction.Amount, 0, 1);
                    return new MatchingPackageSpot($"{reduction.Reason ?? Resources.Reduction} {factor * 100m}%", -originalPrice * factor);
                }
            default:
                return null;
        }
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
                                                             return new MatchingPackageSpot(name, null, part.ShowInMailSpotList ? sortKey : null, part.ShowInMailSpotList);
                                                         })
                                                         .Concat(partMatches.MatchingOptionalRegistrableIds.Select(mtc =>
                                                         {
                                                             var (name, sortKey) = GetRegistrable(registrationId, mtc, part.Registrables!, spots);
                                                             return new MatchingPackageSpot(name, null, part.ShowInMailSpotList ? sortKey : null, part.ShowInMailSpotList);
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
                                                            .ThenByDescending(ppk => ppk.Spots?.Count() ?? 0)
                                                            .ThenByDescending(ppk => ppk.OriginalPrice)
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
                                               pkg.Package.IsCorePackage,
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
            Role ownRole;
            Guid? registrationId_Other;
            if (spot.RegistrationId_Follower == registrationId)
            {
                ownRole = Role.Follower;
                registrationId_Other = spot.RegistrationId;
            }
            else
            {
                ownRole = Role.Leader;
                registrationId_Other = spot.RegistrationId_Follower;
            }

            var text = registrable.Registrable.DisplayName;
            string? partnerName = null;
            if (spot.IsPartnerSpot)
            {
                partnerName = registrationId_Other == null
                                  ? spot.PartnerEmail
                                  : registrations.Where(reg => reg.Id == registrationId_Other)
                                                 .Select(reg => $"{reg.RespondentFirstName} {reg.RespondentLastName}")
                                                 .FirstOrDefault();
            }

            if (partnerName != null)
            {
                text += $" ({enumTranslator.Translate(ownRole)}, {Resources.Partner} {partnerName})";
            }
            else
            {
                text += $" ({enumTranslator.Translate(ownRole)})";
            }

            return (text, registrable.Registrable.ShowInMailListOrder);
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
            PricePackagePartSelectionType.AnyFour  => matchingRegistrableIds.Count == 4,
            PricePackagePartSelectionType.AnyFive  => matchingRegistrableIds.Count == 5,
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
                                           bool IsCorePackage,
                                           IEnumerable<MatchingPackageSpot> Spots,
                                           bool IsReductionsPackage = false);

public record MatchingPackageSpot(string Name,
                                  decimal? PriceAdjustment = null,
                                  int? SortKey = int.MaxValue,
                                  bool ShowInMailSpotList = false)
{
    public decimal? PriceAdjustment { get; set; } = PriceAdjustment;
}
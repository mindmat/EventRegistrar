﻿using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceCalculator
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _tracks;
    private readonly EnumTranslator _enumTranslator;
    private readonly IQueryable<Seat> _spots;
    private readonly IQueryable<PricePackage> _pricePackages;

    public PriceCalculator(IQueryable<Seat> spots,
                           IQueryable<PricePackage> pricePackages,
                           IQueryable<Registration> registrations,
                           IQueryable<Registrable> tracks,
                           EnumTranslator enumTranslator)
    {
        _spots = spots;
        _pricePackages = pricePackages;
        _registrations = registrations;
        _tracks = tracks;
        _enumTranslator = enumTranslator;
    }

    public async Task<(decimal priceOriginal, decimal priceAdmitted, decimal priceAdmittedAndReduced, IReadOnlyCollection<MatchingPackageResult> packagesOriginal,
            IReadOnlyCollection<MatchingPackageResult> packagesAdmitted, bool isOnWaitingList)>
        CalculatePrice(Guid registrationId, CancellationToken cancellationToken = default)
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
            IReadOnlyCollection<MatchingPackageResult> packagesAdmitted, bool isOnWaitingList)>
        CalculatePrice(Registration registration,
                       IEnumerable<Seat> spots)
    {
        var coreTracks = await _tracks.Where(trk => trk.EventId == registration.EventId && trk.IsCore)
                                      .ToListAsync();
        var isOnWaitingList = false;
        var notCancelledSpots = spots.Where(spot => !spot.IsCancelled
                                                 && (spot.RegistrationId == registration.Id
                                                  || spot.RegistrationId_Follower == registration.Id))
                                     .ToList();

        var packages = await _pricePackages.Where(ppg => ppg.EventId == registration.EventId)
                                           .Include(ppg => ppg.Parts!)
                                           .ThenInclude(ppp => ppp.Registrables!)
                                           .ThenInclude(rip => rip.Registrable)
                                           .ToListAsync();
        var (priceOriginal, packagesOriginal, allCoveredOriginal) = CalculatePriceOfSpots(registration.Id, notCancelledSpots, packages, coreTracks);

        var hasSpotsOnWaitingList = notCancelledSpots.Any(spot => spot.IsWaitingList);
        var priceAdmitted = priceOriginal;
        var packagesAdmitted = packagesOriginal;
        var originalPackageIds = packagesOriginal.Select(pkg => pkg.Id).ToList();

        if (hasSpotsOnWaitingList || !allCoveredOriginal)
        {
            isOnWaitingList = true;
            var admittedSpots = notCancelledSpots.Where(spot => !spot.IsWaitingList)
                                                 .ToList();
            (priceAdmitted, packagesAdmitted, var allCoveredAdmitted) = CalculatePriceOfSpots(registration.Id, admittedSpots, packages, coreTracks);

            var samePackages = packagesAdmitted.Select(pkg => pkg.Id).All(pid => originalPackageIds.Contains(pid));
            if (!samePackages)
            {
                var fallbackPackages = packagesAdmitted.Where(adm => !packagesOriginal.Select(ori => ori.Id).Contains(adm.Id))
                                                       .ToList();
                if (fallbackPackages.All(ppk => ppk.AllowAsFallback))
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
            }
        }

        var (priceAdmittedAndReduced, reductionPackage) = GetReducedPrice(priceAdmitted, registration.IndividualReductions);
        if (reductionPackage != null)
        {
            packagesAdmitted = packagesAdmitted.Append(reductionPackage.Value).ToList();
        }

        return (priceOriginal, priceAdmitted, priceAdmittedAndReduced, packagesOriginal, packagesAdmitted, isOnWaitingList);
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
            var reducedPrice = Math.Clamp(overwrite.Amount, 0, priceNotReduced);
            return (reducedPrice, new MatchingPackageResult(null,
                                                            $"{Resources.Reduction}: {overwrite.Reason}",
                                                            reducedPrice - priceNotReduced,
                                                            false,
                                                            Array.Empty<MatchingPackageSpot>()));
        }

        var totalReduction = individualReductions.Select(ird => ird.Amount)
                                                 .Sum();
        var priceAdmittedAndReduced = Math.Clamp(priceNotReduced - totalReduction, 0, priceNotReduced);
        return (priceAdmittedAndReduced, new MatchingPackageResult(null,
                                                                   Resources.Reduction,
                                                                   -totalReduction,
                                                                   false,
                                                                   individualReductions.Select(ird => new MatchingPackageSpot(ird.Reason ?? Resources.Reduction, -ird.Amount))));
    }

    private (decimal Price, IReadOnlyCollection<MatchingPackageResult> matchingPackages, bool allSpotsCovered) CalculatePriceOfSpots(Guid registrationId,
        IReadOnlyCollection<Seat> spots,
        IReadOnlyCollection<PricePackage> packages,
        IReadOnlyCollection<Registrable> coreTracks)
    {
        var bookedRegistrableIds = new HashSet<Guid>(spots.Select(spot => spot.RegistrableId));
        var bookedCoreRegistrableIds = new HashSet<Guid>(spots.Select(spot => spot.RegistrableId).Where(rid => coreTracks.Select(trk => trk.Id).Contains(rid)));
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
                                                             return new MatchingPackageSpot(name, null, sortKey);
                                                         })
                                                         .Concat(partMatches.MatchingOptionalRegistrableIds.Select(mtc =>
                                                         {
                                                             var (name, sortKey) = GetRegistrable(registrationId, mtc, part.Registrables!, spots);
                                                             return new MatchingPackageSpot(name, null, sortKey);
                                                         }))
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
                if (matchingPackage.MatchingRequiredRegistrableId.All(coveredRegistrableIds.Contains))
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

        var notCoveredRegistrableIds = bookedCoreRegistrableIds.Except(matchingPackages.SelectMany(pkg => pkg.MatchingRequiredRegistrableId.Concat(pkg.MatchingOptionalRegistrableId)));
        var price = matchingPackages.Sum(ppk => ppk.Price);
        return (price,
                   matchingPackages.Select(pkg => new MatchingPackageResult
                                           (
                                               pkg.Package.Id,
                                               pkg.Package.Name,
                                               pkg.Price,
                                               pkg.Package.AllowAsFallback,
                                               pkg.Spots
                                           ))
                                   .ToList(),
                   !notCoveredRegistrableIds.Any());
    }

    private (string Name, int? SortKey) GetRegistrable(Guid registrationId,
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
            return ($"{registrable.Registrable.DisplayName} ({_enumTranslator.Translate(role)})", registrable.Registrable.ShowInMailListOrder);
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
                                     IEnumerable<MatchingPackageSpot> Spots);

public record struct MatchingPackageResult(Guid? Id,
                                           string Name,
                                           decimal Price,
                                           bool AllowAsFallback,
                                           IEnumerable<MatchingPackageSpot> Spots);

public record MatchingPackageSpot(string Name,
                                  decimal? PriceAdjustment = null,
                                  int? SortKey = int.MaxValue)
{
    public decimal? PriceAdjustment { get; set; } = PriceAdjustment;
}
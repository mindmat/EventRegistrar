using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceCalculator
{
    private readonly ILogger _logger;
    private readonly IQueryable<Reduction> _reductions;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Seat> _spots;

    public PriceCalculator(ILogger logger,
                           IQueryable<Seat> spots,
                           IQueryable<Registration> registrations,
                           IQueryable<Registrable> registrables,
                           IQueryable<Reduction> reductions)
    {
        _logger = logger;
        _spots = spots;
        _registrations = registrations;
        _registrables = registrables;
        _reductions = reductions;
    }

    public async Task<decimal> CalculatePrice(Guid registrationId)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == registrationId);
        var spots = await _spots.Where(spot => spot.RegistrationId == registrationId
                                            || spot.RegistrationId_Follower == registrationId)
                                .Include(spot => spot.Registrable)
                                .ToListAsync();

        return await CalculatePrice(registration, spots);
    }

    public async Task<decimal> CalculatePrice(Registration registration, IEnumerable<Seat> spots)
    {
        var notCancelledSpots = spots.Where(spot => !spot.IsCancelled &&
                                                    (spot.RegistrationId == registration.Id ||
                                                     spot.RegistrationId_Follower == registration.Id))
                                     .ToList();

        var bookedRegistrableIds = new HashSet<Guid>(notCancelledSpots.Select(seat => seat.RegistrableId));
        var price = 0m;
        foreach (var spot in notCancelledSpots)
        {
            var registrable = spot.Registrable ?? await _registrables.FirstAsync(rbl => rbl.Id == spot.RegistrableId);
            if (registrable == null)
            {
                continue;
            }

            price += registration.IsReduced
                ? registrable.ReducedPrice ?? registrable.Price ?? 0m
                : registrable.Price ?? 0m;
            var roleInThisSpot = spot.RegistrationId_Follower == registration.Id ? Role.Follower : Role.Leader;
            var potentialReductions = await _reductions.Where(red => red.RegistrableId == spot.RegistrableId && !red.ActivatedByReduction)
                                                       .ToListAsync();
            _logger.LogInformation($"potential reductions: {potentialReductions.Count}");

            //var applicableReductions = potentialReductions.Where(red => red.ActivatedByReduction
            //                                                         && registration.IsReduced
            //                                                         && red.RegistrableId1_ReductionActivatedIfCombinedWith == null
            //                                                         && (!red.OnlyForRole.HasValue || red.OnlyForRole == roleInThisSpot))
            //                                              .ToList();

            var applicableReductions = potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue &&
                                                                        bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value)
                                                                     && (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue ||
                                                                         bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value))
                                                                     && (!red.OnlyForRole.HasValue || red.OnlyForRole == roleInThisSpot));

            price -= applicableReductions.Sum(red => red.Amount);
        }

        return price;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class PriceCalculator
    {
        private readonly ILogger _logger;
        private readonly IQueryable<Reduction> _reductions;
        private readonly IQueryable<Registration> _registrations;
        private readonly IQueryable<Response> _responses;
        private readonly IQueryable<Seat> _seats;

        public PriceCalculator(ILogger logger,
                               IQueryable<Seat> seats,
                               IQueryable<Response> responses,
                               IQueryable<Registration> registrations,
                               IQueryable<Reduction> reductions)
        {
            _logger = logger;
            _seats = seats;
            _responses = responses;
            _registrations = registrations;
            _reductions = reductions;
        }

        public async Task<decimal> CalculatePrice(Guid registrationId)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == registrationId);
            var seats = await _seats.Where(seat => seat.RegistrationId == registrationId
                                                || seat.RegistrationId_Follower == registrationId)
                                    .Include(seat => seat.Registrable)
                                    .ToListAsync();
            var responses = await _responses.Where(rsp => rsp.RegistrationId == registrationId)
                                            .ToListAsync();
            if (!responses.Any())
            {
                if (registration.RegistrationId_Partner.HasValue)
                {
                    responses = await _responses.Where(rsp => rsp.RegistrationId == registration.RegistrationId_Partner.Value)
                                                .ToListAsync();
                }
            }

            return await CalculatePrice(registration, responses, seats);
        }

        public async Task<decimal> CalculatePrice(Registration registration, IEnumerable<Response> responses, IEnumerable<Seat> seats)
        {
            var notCancelledSeats = seats.Where(seat => !seat.IsCancelled &&
                                                        (seat.RegistrationId == registration.Id || seat.RegistrationId_Follower == registration.Id))
                                         .ToList();

            var bookedRegistrableIds = new HashSet<Guid>(notCancelledSeats.Select(seat => seat.RegistrableId));
            var price = 0m;
            foreach (var seat in notCancelledSeats)
            {
                price += registration.IsReduced
                            ? (seat.Registrable.Price ?? 0m)
                            : (seat.Registrable.ReducedPrice ?? 0m);
                var roleInThisSpot = seat.RegistrationId_Follower == registration.Id ? Role.Follower : Role.Leader;
                var potentialReductions = await _reductions.Where(red => red.RegistrableId == seat.RegistrableId && !red.ActivatedByReduction).ToListAsync();
                _logger.LogInformation($"potential reductions: {potentialReductions.Count}");

                //var applicableReductions = potentialReductions.Where(red => red.ActivatedByReduction
                //                                                         && registration.IsReduced
                //                                                         && red.RegistrableId1_ReductionActivatedIfCombinedWith == null
                //                                                         && (!red.OnlyForRole.HasValue || red.OnlyForRole == roleInThisSpot))
                //                                              .ToList();

                var applicableReductions = potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue && bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value)
                                                                         && (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue || bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value))
                                                                         && (!red.OnlyForRole.HasValue || red.OnlyForRole == roleInThisSpot));

                price -= applicableReductions.Sum(red => red.Amount);
            }

            return price;
        }
    }
}
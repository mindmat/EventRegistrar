using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Seats;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class PriceCalculator
    {
        private readonly ILogger _logger;
        private readonly IQueryable<Seat> _seats;

        public PriceCalculator(ILogger logger,
                               IQueryable<Seat> seats)
        {
            _logger = logger;
            _seats = seats;
        }

        public async Task<decimal> CalculatePrice(Registration registration)
        {
            var seats = await _seats.Where(seat => (seat.RegistrationId == registration.Id
                                                    || seat.RegistrationId_Follower == registration.Id)
                                                   && !seat.IsCancelled)
                .Include(seat => seat.Registrable)
                //.Include(seat => seat.Registrable.Reductions)
                .ToListAsync();
            var responses = registration.Responses
                .Where(rsp => rsp.RegistrationId == registration.Id);

            var price = await CalculatePrice(responses, seats);

            //if (savePrice)
            //{
            //    registration.Price = price;
            //    if (registration.State != RegistrationState.Cancelled)
            //    {
            //        var paidAmount = (decimal?)registration.Payments.Sum(ass => ass.Amount);
            //        registration.State = (paidAmount ?? 0m) >= price && price > 0m && registration.State != RegistrationState.Paid
            //            ? RegistrationState.Paid
            //            : RegistrationState.Received;
            //    }
            //    await dbContext.SaveChangesAsync();
            //}
            return price;
        }

        public async Task<decimal> CalculatePrice(IEnumerable<Response> responses, IEnumerable<Seat> seats)
        {
            var notCancelledSeats = seats.Where(seat => !seat.IsCancelled).ToList();
            var registrationQuestionOptionIds = responses.Where(rsp => rsp.QuestionOptionId.HasValue)
                                                         .Select(rsp => rsp.QuestionOptionId.Value)
                                                         .ToList();

            var bookedRegistrableIds = new HashSet<Guid>(notCancelledSeats.Select(seat => seat.RegistrableId));
            var price = 0m;
            foreach (var seat in notCancelledSeats)
            {
                price += seat.Registrable.Price ?? 0m;

                //_logger.LogInformation($"reductions: {seat.Registrable.Reductions.Count}");
                //var potentialReductions = seat.Registrable.Reductions.ToList();

                //var applicableReductions = potentialReductions.Where(red => red.QuestionOptionId_ActivatesReduction.HasValue &&
                //                                                            registrationQuestionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value) &&
                //                                                            !red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue)
                //                                              .ToList();

                //applicableReductions.AddRange(potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue && bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value) &&
                //                                                               (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue || bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value)) &&
                //                                                               (!red.QuestionOptionId_ActivatesReduction.HasValue || registrationQuestionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value))));

                //price -= applicableReductions.Sum(red => red.Amount);
            }

            return price;
        }
    }
}
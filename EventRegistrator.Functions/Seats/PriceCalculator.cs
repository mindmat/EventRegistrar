using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Seats
{
   public class PriceCalculator
   {
      public static async Task<decimal> CalculatePrice(Guid registrationId, bool savePrice = false, TraceWriter log = null)
      {
         using (var dbContext = new EventRegistratorDbContext())
         {
            var registration = await dbContext.Registrations.FirstOrDefaultAsync(reg => reg.Id == registrationId);
            if (registration == null)
            {
               throw new ArgumentException($"Registration with ID {registrationId} not found");
            }
            var seats = await dbContext.Seats
                                       .Where(seat => seat.RegistrationId == registrationId ||
                                                      seat.RegistrationId_Follower == registrationId)
                                       .Include(seat => seat.Registrable)
                                       .Include(seat => seat.Registrable.Reductions)
                                       .ToListAsync();

            var registrationQuestionOptionIds = await dbContext.Responses
                                                               .Where(rsp => rsp.RegistrationId == registrationId && rsp.QuestionOptionId.HasValue)
                                                               .Select(rsp => rsp.QuestionOptionId.Value)
                                                               .ToListAsync();
            log?.Info($"seats: {seats.Count}");
            var bookedRegistrableIds = new HashSet<Guid>(seats.Select(seat => seat.RegistrableId));
            var price = 0m;
            foreach (var seat in seats)
            {
               log?.Info($"reductions: {seat.Registrable.Reductions.Count}");

               price += seat.Registrable.Price ?? 0m;
               var potentialReductions = seat.Registrable.Reductions.ToList();

               var applicableReductions = potentialReductions.Where(red => red.QuestionOptionId_ActivatesReduction.HasValue &&
                                                                           registrationQuestionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value) &&
                                                                           !red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue)
                                                             .ToList();

               applicableReductions.AddRange(potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue && bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value) &&
                                                                              (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue || bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value)) &&
                                                                              (!red.QuestionOptionId_ActivatesReduction.HasValue || registrationQuestionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value))));

               price -= applicableReductions.Sum(red => red.Amount);
            }

            if (savePrice)
            {
               registration.Price = price;
               await dbContext.SaveChangesAsync();
            }

            return price;
         }
      }
   }
}
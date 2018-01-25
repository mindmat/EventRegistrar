using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class GetPaymentOverview
    {
        [FunctionName("GetPaymentOverview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{eventIdString}/PaymentOverview")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(eventIdString, out var eventId))
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{eventIdString} is not a guid");
            }

            using (var dbContext = new EventRegistratorDbContext())
            {
                var balance = await dbContext.PaymentFiles
                                             .Where(pmf => pmf.EventId == eventId)
                                             .OrderByDescending(pmf => pmf.BookingsTo ?? DateTime.MinValue)
                                             .Select(pmf => new
                                             {
                                                 pmf.AccountIban,
                                                 pmf.Balance,
                                                 pmf.Currency,
                                                 Date = pmf.BookingsTo
                                             })
                                             .FirstOrDefaultAsync();

                var activeRegistrations = await dbContext.Registrations
                                                         .Where(reg => reg.IsWaitingList != true &&
                                                                       reg.State != RegistrationState.Cancelled)
                                                         .Select(reg => new
                                                         {
                                                             reg.Id,
                                                             reg.State,
                                                             reg.Price,
                                                             Paid = (decimal?)reg.Payments.Sum(ass => ass.Amount)
                                                         })
                                                         .ToListAsync();

                var registrables = await dbContext.Registrables
                                                  .Where(rbl => (rbl.MaximumDoubleSeats.HasValue || rbl.MaximumSingleSeats.HasValue) && rbl.Price.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Select(rbl => new
                                                  {
                                                      RegistrableId = rbl.Id,
                                                      rbl.Name,
                                                      Price = rbl.Price.Value,
                                                      SpotsAvailable = rbl.MaximumSingleSeats ?? rbl.MaximumDoubleSeats.Value * 2,
                                                      LeaderCount = rbl.Seats.Where(seat => !seat.IsCancelled && !seat.IsWaitingList).Count(seat => seat.RegistrationId != null),
                                                      FollowerCount = rbl.Seats.Where(seat => !seat.IsCancelled && !seat.IsWaitingList).Count(seat => seat.RegistrationId_Follower != null),
                                                  })
                                                  .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    Balance = new
                    {
                        balance.Balance,
                        balance.Currency,
                        balance.AccountIban,
                        balance.Date?.Date
                    },
                    ReceivedMoney = activeRegistrations.Sum(reg => reg.Paid ?? 0m),
                    PaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Paid),
                    OutstandingAmount = activeRegistrations.Where(reg => reg.State == RegistrationState.Received).Sum(reg => (reg.Price ?? 0m) - (reg.Paid ?? 0m)),
                    NotFullyPaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Received),
                    PotentialOfOpenSpots = registrables.Select(rbl => new
                    {
                        rbl.RegistrableId,
                        rbl.Name,
                        SpotsAvailable = rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount,
                        PotentialIncome = (rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount) * rbl.Price
                    })
                });
            }
        }
    }
}
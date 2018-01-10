using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Registrables
{
    public static class SingleRegistrableOverview
    {
        [FunctionName("SingleRegistrableOverview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //// parse query parameter
            //string name = req.GetQueryNameValuePairs()
            //    .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //    .Value;

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrables = await dbContext.Registrables
                                                  .Where(rbl => !rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync();

                var registrationsOnWaitingList = new HashSet<Guid>(dbContext.Registrations.Where(reg => reg.IsWaitingList ?? false).Select(reg => reg.Id));

                return req.CreateResponse(HttpStatusCode.OK, registrables.OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue).Select(rbl => new
                {
                    rbl.Id,
                    rbl.Name,
                    SpotsAvailable = rbl.MaximumSingleSeats,
                    Accepted = rbl.Seats.Count(seat => !seat.IsCancelled && !seat.IsWaitingList && !registrationsOnWaitingList.Contains(seat.RegistrationId ?? Guid.Empty)),
                    OnWaitingList = rbl.HasWaitingList ? (int?)rbl.Seats.Count(seat => !seat.IsCancelled && (seat.IsWaitingList || registrationsOnWaitingList.Contains(seat.RegistrationId ?? Guid.Empty))) : null
                }));
            }
        }
    }
}
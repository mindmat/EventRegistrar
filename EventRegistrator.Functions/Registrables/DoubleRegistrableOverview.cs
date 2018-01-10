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
    public static class DoubleRegistrableOverview
    {
        [FunctionName("DoubleRegistrableOverview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //// parse query parameter
            //string name = req.GetQueryNameValuePairs()
            //    .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //    .Value;

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrables = await dbContext.Registrables
                                                  .Where(rbl => rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, registrables.Select(rbl => new
                {
                    rbl.Id,
                    rbl.Name,
                    SpotsAvailable = rbl.MaximumDoubleSeats,
                    LeadersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled && seat.RegistrationId.HasValue && !seat.IsWaitingList),
                    FollowersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled && seat.RegistrationId_Follower.HasValue && !seat.IsWaitingList),
                    LeadersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled && seat.RegistrationId.HasValue && seat.IsWaitingList),
                    FollowersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled && seat.RegistrationId_Follower.HasValue && seat.IsWaitingList),
                }));
            }
        }
    }
}
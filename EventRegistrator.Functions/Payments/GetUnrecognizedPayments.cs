using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class GetUnrecognizedPayments
    {
        [FunctionName("GetUnrecognizedPayments")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{eventIdString}/payments")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            if (!Guid.TryParse(eventIdString, out var eventId))
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{eventIdString} is not a guid");
            }

            using (var dbContext = new EventRegistratorDbContext())
            {
                var payments = await dbContext.ReceivedPayments
                                              .Where(rpy => rpy.PaymentFile.EventId == eventId &&
                                                            rpy.RecognizedEmail == null && rpy.Amount - (rpy.Repaid ?? 0m) > 0)
                                              .Select(rpy => new
                                              {
                                                  rpy.Id,
                                                  rpy.Amount,
                                                  Assigned = (decimal?)rpy.Assignments.Sum(ass => ass.Amount),
                                                  rpy.BookingDate,
                                                  rpy.Currency,
                                                  rpy.Info,
                                                  rpy.Reference,
                                                  rpy.Repaid,
                                                  rpy.Settled
                                              })
                                              .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, payments);
            }
        }
    }
}

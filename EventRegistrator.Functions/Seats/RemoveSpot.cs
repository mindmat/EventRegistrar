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

namespace EventRegistrator.Functions.Seats
{
   public static class RemoveSpot
   {
      [FunctionName("RemoveSpot")]
      public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/{registrationIdString:guid}/removeSpot")]
        HttpRequestMessage req,
        string registrationIdString,
        TraceWriter log)
      {
         var registrationId = Guid.Parse(registrationIdString);
         var registrableId = Guid.Parse(req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "registrableId", StringComparison.OrdinalIgnoreCase) == 0).Value);

         using (var dbContext = new EventRegistratorDbContext())
         {
            var seatToRemove = await dbContext.Seats.FirstOrDefaultAsync(seat => seat.RegistrableId == registrableId &&
                                                                                 seat.RegistrationId == registrationId &&
                                                                                 seat.RegistrationId_Follower == null &&
                                                                                 seat.PartnerEmail == null);

            dbContext.Seats.Remove(seatToRemove);
            await dbContext.SaveChangesAsync();

            await PriceCalculator.CalculatePrice(registrationId, true);

            return req.CreateResponse(HttpStatusCode.OK);
         }
      }
   }
}

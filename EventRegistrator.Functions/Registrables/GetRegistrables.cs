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

namespace EventRegistrator.Functions.Registrables
{
  public static class GetRegistrables
  {
    [FunctionName("GetRegistrables")]
    public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/registrables")]
      HttpRequestMessage req,
      string eventIdString,
      TraceWriter log)
    {
      var eventId = Guid.Parse(eventIdString);

      using (var dbContext = new EventRegistratorDbContext())
      {
        var registrables = await dbContext.Registrables
                                          .Where(rbl => rbl.EventId == eventId)
                                          .Select(rbl => new
                                          {
                                            rbl.Id,
                                            rbl.Name,
                                            rbl.HasWaitingList,
                                            IsDoubleRegistrable = rbl.MaximumDoubleSeats.HasValue,
                                            rbl.ShowInMailListOrder
                                          })
                                          .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                          .ToListAsync();

        return req.CreateResponse(HttpStatusCode.OK, registrables);
      }
    }
  }
}

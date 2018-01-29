using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.WaitingList
{
  public static class TryPromoteFromWaitingListHttpHandler
  {
    [FunctionName("TryPromoteFromWaitingListHttpHandler")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrables/{registrableIdString:guid}/TryPromoteFromWaitingList")]
        HttpRequestMessage req,
        string registrableIdString, 
        TraceWriter log)
    {
      log.Info($"TryPromoteFromWaitingList for registrable {registrableIdString}");

      var registrableId = Guid.Parse(registrableIdString);

      await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand { RegistrableId = registrableId }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);

      return req.CreateResponse(HttpStatusCode.OK, "Command is queued");
    }
  }
}
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
  public static class GetRegistrationSpots
  {
    private class Spot
    {
      public Guid Id { get; set; }
      public Guid RegistrableId { get; set; }
      public string Registrable { get; set; }
      public int? SortKey { get; set; }
      public Guid? PartnerRegistrationId { get; set; }
      public DateTime FirstPartnerJoined { get; set; }
      public string Partner { get; set; }
      public bool IsCore { get; set; }
    }

    [FunctionName("GetRegistrationSpots")]
    public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations/{registrationIdString:guid}/spots")]
            HttpRequestMessage req,
        string registrationIdString,
        TraceWriter log)
    {
      if (!Guid.TryParse(registrationIdString, out var registrationId))
      {
        return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{registrationIdString} is not a guid");
      }

      using (var dbContext = new EventRegistratorDbContext())
      {
        var spots = await dbContext.Seats.Where(seat => seat.RegistrationId == registrationId || seat.RegistrationId_Follower == registrationId)
                                         .Where(seat => !seat.IsCancelled)
                                         .Select(seat => new Spot
                                         {
                                           Id = seat.Id,
                                           RegistrableId = seat.RegistrableId,
                                           Registrable = seat.Registrable.Name,
                                           SortKey = seat.Registrable.ShowInMailListOrder,
                                           PartnerRegistrationId = seat.PartnerEmail != null ?
                                                                        seat.RegistrationId == registrationId ?
                                                                           seat.RegistrationId_Follower :
                                                                           seat.RegistrationId :
                                                                        null,
                                           FirstPartnerJoined = seat.FirstPartnerJoined,
                                           IsCore = seat.Registrable.IsCore
                                         })
                                         .OrderBy(seat => seat.SortKey)
                                         .ToListAsync();

        foreach (var spot in spots.Where(spot => spot.PartnerRegistrationId != null))
        {
          var names = await dbContext.Registrations
                                     .Where(reg => reg.Id == spot.PartnerRegistrationId)
                                     .Select(reg => new { reg.RespondentFirstName, reg.RespondentLastName })
                                     .FirstOrDefaultAsync();
          spot.Partner = $"{names.RespondentFirstName} {names.RespondentLastName}";
        }
        return req.CreateResponse(HttpStatusCode.OK, spots);
      }
    }
  }
}
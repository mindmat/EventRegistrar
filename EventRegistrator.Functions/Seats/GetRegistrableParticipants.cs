using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Seats
{
    public static class GetRegistrableParticipants
    {
        [FunctionName("GetRegistrableParticipants")]
        public static async System.Threading.Tasks.Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrables/{registrableIdString}/participants")]
            HttpRequestMessage req,
            string registrableIdString,
            TraceWriter log)
        {
            if (!Guid.TryParse(registrableIdString, out var registrableId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"{registrableIdString} should be a GUID");
            }

            log.Info("C# HTTP trigger function processed a request.");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrable = await dbContext.Registrables.FirstOrDefaultAsync(rbl => rbl.Id == registrableId);
                if (registrable == null)
                {
                    return req.CreateErrorResponse(HttpStatusCode.NotFound, $"No registrable found with id {registrableId}");
                }

                var participants = await dbContext
                                         .Seats
                                         .Where(seat => seat.RegistrableId == registrableId &&
                                                        !seat.IsCancelled)
                                         .OrderBy(seat => seat.IsWaitingList)
                                         .ThenBy(seat => seat.FirstPartnerJoined)
                                         .Select(seat => new PlaceDisplayInfo
                                         {
                                             Leader = seat.RegistrationId == null ? null : new RegistrationDisplayInfo
                                             {
                                                 Id = seat.Registration.Id,
                                                 Email = seat.Registration.RespondentEmail,
                                                 State = seat.Registration.State,
                                                 FirstName = seat.Registration.RespondentFirstName,
                                                 LastName = seat.Registration.RespondentLastName
                                             },
                                             Follower = seat.RegistrationId_Follower == null ? null : new RegistrationDisplayInfo
                                             {
                                                 Id = seat.Registration_Follower.Id,
                                                 Email = seat.Registration_Follower.RespondentEmail,
                                                 State = seat.Registration_Follower.State,
                                                 FirstName = seat.Registration_Follower.RespondentFirstName,
                                                 LastName = seat.Registration_Follower.RespondentLastName
                                             },
                                             IsOnWaitingList = seat.IsWaitingList || seat.Registration != null && seat.Registration.IsWaitingList == true,
                                             IsPartnerRegistration = seat.PartnerEmail != null
                                         })
                                         .ToListAsync();

                log.Info($"{participants.Count} participants found");

                return req.CreateResponse(HttpStatusCode.OK, new RegistrableDisplayInfo
                {
                    Name = registrable.Name,
                    MaximumDoubleSeats = registrable.MaximumDoubleSeats,
                    MaximumSingleSeats = registrable.MaximumSingleSeats,
                    MaximumAllowedImbalance = registrable.MaximumAllowedImbalance,
                    HasWaitingList = registrable.HasWaitingList,
                    Participants = participants.Where(prt => !prt.IsOnWaitingList),
                    WaitingList = participants.Where(prt => prt.IsOnWaitingList)
                });
            }
        }
    }
}
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.WaitingList;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Registrations
{
    public static class CancelRegistration
    {
        [FunctionName("CancelRegistration")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CancelRegistration/{registrationIdString}")]HttpRequestMessage req, string registrationIdString, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var registrationId = Guid.Parse(registrationIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registration = await dbContext.Registrations
                                                  .Include(reg => reg.Payments)
                                                  .Include(reg => reg.RegistrationForm)
                                                  .FirstOrDefaultAsync(reg => reg.Id == registrationId);
                if (registration == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound, $"No registration with id {registrationId}");
                }

                if (registration.Payments.Any())
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"There are already payments for registration {registrationId}");
                }
                if (registration.State == RegistrationState.Cancelled)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"Registration {registrationId} is already cancelled");
                }
                if (registration.State == RegistrationState.Paid)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"Registration {registrationId} is already paid and cannot be cancelled anymore");
                }

                registration.State = RegistrationState.Cancelled;
                var places = await dbContext.Seats.Where(plc => plc.RegistrationId == registrationId || plc.RegistrationId_Follower == registrationId).ToListAsync();
                foreach (var place in places.Where(plc => plc.RegistrationId == registrationId))
                {
                    if (place.RegistrationId_Follower.HasValue)
                    {
                        // double place, leave the partner in
                        place.RegistrationId = null;
                    }
                    else
                    {
                        // single place, remove the place
                        dbContext.Seats.Remove(place);
                    }
                }
                foreach (var place in places.Where(plc => plc.RegistrationId_Follower == registrationId))
                {
                    if (place.RegistrationId.HasValue)
                    {
                        // double place, leave the partner in
                        place.RegistrationId_Follower = null;
                    }
                    else
                    {
                        // single place, remove the place
                        dbContext.Seats.Remove(place);
                    }
                }
                await dbContext.SaveChangesAsync();

                if (registration.RegistrationForm.EventId.HasValue)
                {
                    foreach (var place in places)
                    {
                        await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand
                        {
                            EventId = registration.RegistrationForm.EventId.Value,
                            RegistrableId = place.RegistrableId
                        }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
                    }
                }

                await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            }

            return req.CreateResponse(HttpStatusCode.OK, "Registration cancelled");
        }
    }
}
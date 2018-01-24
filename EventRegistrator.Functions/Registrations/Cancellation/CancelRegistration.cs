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

namespace EventRegistrator.Functions.Registrations.Cancellation
{
    public static class CancelRegistration
    {
        [FunctionName("CancelRegistration")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Registration/{registrationIdString}/Cancel")]
            HttpRequestMessage req, 
            string registrationIdString, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var registrationId = Guid.Parse(registrationIdString);

            var ignorePayments = req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "ignorePayments", StringComparison.OrdinalIgnoreCase) == 0).Value == "true";
            var reason = req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "reason", StringComparison.OrdinalIgnoreCase) == 0).Value;
            decimal.TryParse(req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "refundPercentage", StringComparison.OrdinalIgnoreCase) == 0).Value, out var refundPercentage);
            if (refundPercentage < 0m)
            {
                refundPercentage = 0m;
            }
            else if (refundPercentage > 1m)
            {
                refundPercentage = 1m;
            }

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

                if (registration.Payments.Any() && !ignorePayments)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"There are already payments for registration {registrationId}");
                }
                if (registration.State == RegistrationState.Cancelled)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"Registration {registrationId} is already cancelled");
                }
                if (registration.State == RegistrationState.Paid && !ignorePayments)
                {
                    return req.CreateResponse(HttpStatusCode.PreconditionFailed, $"Registration {registrationId} is already paid and cannot be cancelled anymore");
                }

                registration.State = RegistrationState.Cancelled;
                var places = await dbContext.Seats
                                            .Where(plc => plc.RegistrationId == registrationId || plc.RegistrationId_Follower == registrationId)
                                            .ToListAsync();
                foreach (var place in places.Where(plc => plc.RegistrationId == registrationId))
                {
                    if (place.RegistrationId_Follower.HasValue)
                    {
                        // double place, leave the partner in
                        place.RegistrationId = null;
                        place.PartnerEmail = null;
                    }
                    else
                    {
                        // single place, cancel the place
                        place.IsCancelled = true;
                    }
                }
                foreach (var place in places.Where(plc => plc.RegistrationId_Follower == registrationId))
                {
                    if (place.RegistrationId.HasValue)
                    {
                        // double place, leave the partner in
                        place.RegistrationId_Follower = null;
                        place.PartnerEmail = null;
                    }
                    else
                    {
                        // single place, cancel the place
                        place.IsCancelled = true;
                    }
                }

                var cancellation = new RegistrationCancellation
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = registrationId,
                    Reason = reason,
                    Created = DateTime.UtcNow,
                    RefundPercentage = refundPercentage,
                    Refund = refundPercentage * registration.Payments.Sum(ass=>ass.Amount)
                };
                dbContext.RegistrationCancellations.Add(cancellation);

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

                await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId, Withhold = true }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            }

            return req.CreateResponse(HttpStatusCode.OK, "Registration cancelled");
        }
    }
}
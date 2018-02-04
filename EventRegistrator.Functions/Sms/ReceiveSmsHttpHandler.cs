using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Twilio.TwiML;

namespace EventRegistrator.Functions.Sms
{
    public static class ReceiveSmsHttpHandler
    {
        [FunctionName("ReceiveSmsHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sms/reply")]
            HttpRequestMessage req,
            TraceWriter log)
        {
            var form = await req.Content.ReadAsFormDataAsync();
            var data = FormDeserializer.Read<TwilioSms>(form);
            log.Info($"Content: {JsonConvert.SerializeObject(data)}");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var eventIds = await dbContext.Events
                                              .Where(evt => evt.TwilioAccountSid == data.AccountSid && evt.State != State.Finished)
                                              .Select(evt => (Guid?)evt.Id)
                                              .ToListAsync();

                var registrations = await dbContext.Registrations
                                                   .Where(reg => eventIds.Contains(reg.RegistrationForm.EventId)
                                                              && reg.PhoneNormalized == data.From)
                                                   .ToListAsync();
                var registrationId = registrations.Count == 1
                    ? registrations.FirstOrDefault()?.Id
                    : registrations.FirstOrDefault(reg => reg.State != RegistrationState.Cancelled)?.Id;

                var sms = new Sms
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = registrationId,
                    SmsSid = data.SmsSid,
                    SmsStatus = data.SmsStatus,
                    Body = data.Body,
                    From = data.From,
                    To = data.To,
                    AccountSid = data.AccountSid,
                    RawData = JsonConvert.SerializeObject(data),
                    Received = DateTime.UtcNow,
                };

                dbContext.Sms.Add(sms);

                await dbContext.SaveChangesAsync();
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}

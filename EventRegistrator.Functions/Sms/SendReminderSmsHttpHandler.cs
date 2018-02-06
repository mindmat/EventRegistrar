using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EventRegistrator.Functions.Sms
{
    public static class SendReminderSmsHttpHandler
    {
        [FunctionName("SendReminderSmsHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/{registrationIdString:guid}/sendReminderSms")]
           HttpRequestMessage req,
           string registrationIdString,
           TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var alreadySent = await dbContext.Sms.AnyAsync(s => s.RegistrationId == registrationId
                                                                    && s.Type == SmsType.Reminder);
                if (alreadySent)
                {
                    log.Error("Reminder already sent");
                    return req.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Reminder already sent");
                }
                var language = await dbContext.Registrations
                    .Where(reg => reg.Id == registrationId)
                    .Select(reg => reg.Language)
                    .FirstOrDefaultAsync();


                var template = language == "de"
                    ? @"Hallo {{FirstName}}, hast du die Mails vom Leapin' Lindy an {{EMail}} erhalten? Bitte melde dich in den nächsten 24h, ob du immer noch dabei bist oder deine Anmeldung stornieren willst. Liebe Grüsse, das Leapin' Lindy-Team"
                    : @"Hello {{FirstName}}, did you receive the mails from Leapin' Lindy to {{EMail}}? Please let us know in the next 24 hours whether you are still in or want to cancel your registration. Regards, the Leapin' Lindy Team";

                var body = await new TemplateParameterFinder().Fill(template, registrationId);
                return await SmsSender.SendSms(registrationId, body, req, log);
            }
        }
    }
}
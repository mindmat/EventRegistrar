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
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var keyVaultUrl = Environment.GetEnvironmentVariable("KeyVaultUri");
            var twilioSid = (await kvClient.GetSecretAsync(keyVaultUrl, "TwilioSID"))?.Value;
            var twilioToken = (await kvClient.GetSecretAsync(keyVaultUrl, "TwilioToken"))?.Value;

            if (twilioSid == null || twilioToken == null)
            {
                log.Error("No Twilio SID/Token found");
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "No Twilio SID/Token found");
            }

            using (var dbContext = new EventRegistratorDbContext())
            {
                var alreadySent = await dbContext.Sms.AnyAsync(s => s.RegistrationId == registrationId 
                                                                 && s.Type == SmsType.Reminder);
                if (alreadySent)
                {
                    log.Error("Reminder already sent");
                    return req.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Reminder already sent");
                }
                var registration = await dbContext.Registrations
                                                  .Where(reg => reg.Id == registrationId)
                                                  .Select(reg => new { reg.PhoneNormalized, reg.Language })
                                                  .FirstOrDefaultAsync();

                if (registration?.PhoneNormalized == null)
                {
                    log.Error("No number found in registration");
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "No number found in registration");
                }

                TwilioClient.Init(twilioSid, twilioToken);
                var fromNumber = Environment.GetEnvironmentVariable("TwilioNumber");

                var template = registration.Language == "de"
                    ? @"Hallo {{FirstName}}, hast du die Mails vom Leapin' Lindy an {{EMail}} erhalten? Bitte melde dich in den nächsten 24h, ob du immer noch dabei bist oder deine Anmeldung stornieren willst. Liebe Grüsse, das Leapin' Lindy-Team"
                    : @"Hello {{FirstName}}, did you receive the mails from Leapin' Lindy to {{EMail}}? Please let us know in the next 24 hours whether you are still in or want to cancel your registration. Regards, the Leapin' Lindy Team";

                var body = await new TemplateParameterFinder().Fill(template, registrationId);

                var callbackUrl = new Uri($"{req.RequestUri.Scheme}://{req.RequestUri.Authority}/api/sms/status");

                var message = await MessageResource.CreateAsync("+41798336129", from: fromNumber, body: body, statusCallback: callbackUrl);

                var sms = new Sms
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = registrationId,
                    SmsSid = message.Sid,
                    SmsStatus = message.Status.ToString(),
                    Body = message.Body,
                    From = message.From.ToString(),
                    To = message.To,
                    AccountSid = message.AccountSid,
                    Sent = DateTime.UtcNow,
                    Price = $"{message.Price}{message.PriceUnit}",
                    ErrorCode = message.ErrorCode,
                    Error = message.ErrorMessage,
                    Type = SmsType.Reminder,
                    RawData = JsonConvert.SerializeObject(message)
                };

                dbContext.Sms.Add(sms);

                await dbContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK, message);
            }
        }
    }
}
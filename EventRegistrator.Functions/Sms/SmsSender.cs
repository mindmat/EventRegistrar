using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace EventRegistrator.Functions.Sms
{
    public class SmsSender
    {
        public static async Task<HttpResponseMessage> SendSms(Guid registrationId, string body, HttpRequestMessage req, TraceWriter log)
        {
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
                var phone = await dbContext.Registrations
                                           .Where(reg => reg.Id == registrationId)
                                           .Select(reg => reg.PhoneNormalized)
                                           .FirstOrDefaultAsync();

                if (phone == null)
                {
                    log.Error("No number found in registration");
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "No number found in registration");
                }

                TwilioClient.Init(twilioSid, twilioToken);
                var fromNumber = Environment.GetEnvironmentVariable("TwilioNumber");

                var callbackUrl = new Uri($"{req.RequestUri.Scheme}://{req.RequestUri.Authority}/api/sms/status");

                var message = await MessageResource.CreateAsync(phone,
                                                                from: fromNumber,
                                                                body: body,
                                                                statusCallback: callbackUrl);

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
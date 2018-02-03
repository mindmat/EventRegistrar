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
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EventRegistrator.Functions.Sms
{
    public static class SendSmsHttpHandler
    {
        [FunctionName("SendSmsHttpHandler")]
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
                var number = dbContext.Registrations
                    .Where(reg => reg.Id == registrationId)
                    .Select(reg => reg.Phone)
                    .FirstOrDefaultAsync();

                if (number == null)
                {
                    log.Error("No number found in registration");
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "No number found in registration");
                }

                TwilioClient.Init(twilioSid, twilioToken);
                var fromNumber = Environment.GetEnvironmentVariable("TwilioNumber");
                //var body =
                var message = await MessageResource.CreateAsync("+41798336129", from: fromNumber, body: "Hello World through twilio");
                log.Info($"{message.Sid}, {message.Price}, {message.PriceUnit}, {message.ErrorCode}, {message}");
                return req.CreateResponse(HttpStatusCode.OK, message);
            }
        }
    }
}
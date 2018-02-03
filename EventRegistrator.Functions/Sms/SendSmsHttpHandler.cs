using System;
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
using Twilio.Clients;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
            var twilioSid = (await kvClient.GetSecretAsync(Environment.GetEnvironmentVariable("TwilioSID"))).Value;
            var twilioToken = (await kvClient.GetSecretAsync(Environment.GetEnvironmentVariable("TwilioToken"))).Value;


            log.Info($"sid {twilioSid}, token {twilioToken}");

            //var keyVault = new KeyVaultClient(async (authority, resource, scope) => {
            //    var authContext = new AuthenticationContext(authority);
            //    var credential = new ClientCredential(adClientId, adKey);
            //    var token = await authContext.AcquireTokenAsync(resource, credential);

            //    return token.AccessToken;
            //});

            // Get the API key out of the vault
            //var apiKey = keyVault.GetSecretAsync(keyUrl).Result.Value;
            TwilioClient.Init(twilioSid, twilioToken);
            var message = await MessageResource.CreateAsync("+41798336129", from: "+41 79 807 42 62 ", body: "Hello World through twilio");
            
            log.Info($"{message.Sid}, {message.Price}, {message.PriceUnit}, {message.ErrorCode}, {message}");

            //using (var dbContext = new EventRegistratorDbContext())
            //{
            //    var smsClient = new TwilioRestClient(twilioSid, ) TwilioClient.GetRestClient();
            //}
            return req.CreateResponse(HttpStatusCode.OK, message);
        }
    }
}

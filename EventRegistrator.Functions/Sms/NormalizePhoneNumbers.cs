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

namespace EventRegistrator.Functions.Sms
{
    public static class NormalizePhoneNumbers
    {
        [FunctionName("NormalizePhoneNumbers")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventIdString:guid}/normalizePhoneNumbers")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrationsWithPhoneNumber = await dbContext.Registrations
                                                                  .Where(reg => reg.RegistrationForm.EventId == eventId
                                                                             && reg.Phone != null
                                                                             && reg.Phone != string.Empty)
                                                                  .ToListAsync();

                foreach (var registration in registrationsWithPhoneNumber)
                {
                    registration.PhoneNormalized = NormalizePhone(registration.Phone);
                }

                var affectedRows = await dbContext.SaveChangesAsync();
                return req.CreateResponse(HttpStatusCode.OK, $"{affectedRows} Registrations updated");
            }
        }

        public static string NormalizePhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return phone;
            }
            var phoneNormalized = phone.Replace(" ", "").Replace("-", "");

            // Replace 0041 with +41
            if (phoneNormalized.StartsWith("00"))
            {
                phoneNormalized = "+" + phoneNormalized.Remove(0, 2);
            }

            // add swiss prefix: 079 123 45 67 -> +41 79 123 45 67
            if (phoneNormalized.StartsWith("07") && phoneNormalized.Length == 10)
            {
                phoneNormalized = "+41" + phoneNormalized.Remove(0, 1);
            }
            return phoneNormalized;
        }
    }
}

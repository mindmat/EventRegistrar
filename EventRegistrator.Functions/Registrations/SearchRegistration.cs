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

namespace EventRegistrator.Functions.Registrations
{
    public static class SearchRegistration
    {
        [FunctionName("SearchRegistration")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{eventIdString:guid}/registrations")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            if (!Guid.TryParse(eventIdString, out var eventId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"{eventIdString} should be a GUID");
            }

            // parse query parameter
            string searchString = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "searchstring", StringComparison.InvariantCultureIgnoreCase) == 0)
                .Value;

            using (var dbContext = new EventRegistratorDbContext())
            {
                var results = await dbContext.Registrations
                                             .Where(reg => reg.RegistrationForm.EventId == eventId)
                                             .Select(reg => new
                                             {
                                                 Registration = reg,
                                                 MatchResponses = reg.Responses.Where(rsp => rsp.ResponseString.Contains(searchString))
                                             })
                                             .Where(reg => reg.Registration.RespondentEmail.Contains(searchString) ||
                                                           reg.MatchResponses.Any())
                                             .Select(tmp => new
                                             {
                                                 tmp.Registration.Id,
                                                 Email = tmp.Registration.RespondentEmail,
                                                 FirstName = tmp.Registration.RespondentFirstName,
                                                 LastName = tmp.Registration.RespondentLastName,
                                                 Responses = tmp.MatchResponses.Select(rsp => new { Question = rsp.Question.Title, Response = rsp.ResponseString })
                                             })
                                             .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK , results);
            }
        }
    }
}

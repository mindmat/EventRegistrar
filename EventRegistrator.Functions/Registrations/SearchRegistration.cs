using System;
using System.Collections.Generic;
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
            if (!Guid.TryParse(eventIdString, out var eventId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"{eventIdString} should be a GUID");
            }

            // parse query parameter
            string searchString = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "searchstring", StringComparison.InvariantCultureIgnoreCase) == 0)
                .Value;

            log.Info($"Searching in event {eventId} for {searchString}");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var responseMatches = await dbContext.Responses
                                                     .Where(rsp => rsp.Registration.RegistrationForm.EventId == eventId &&
                                                                   rsp.ResponseString.Contains(searchString))
                                                     .Select(rsp => new { rsp.RegistrationId, rsp.Question.Title, rsp.ResponseString })
                                                     .ToListAsync();
                var registrationIds = responseMatches.Select(rsp => rsp.RegistrationId).Distinct().ToList();

                var results = await dbContext.Registrations
                                             .Where(reg => reg.RegistrationForm.EventId == eventId)
                                             .Where(reg => reg.RespondentEmail.Contains(searchString) ||
                                                           registrationIds.Contains(reg.Id))
                                             .Select(reg => new RegistrationMatch
                                             {
                                                 Id = reg.Id,
                                                 Email = reg.RespondentEmail,
                                                 FirstName = reg.RespondentFirstName,
                                                 LastName = reg.RespondentLastName
                                             })
                                             .ToListAsync();

                foreach (var registrationMatch in results)
                {
                    registrationMatch.Responses = responseMatches.Where(rsp => rsp.RegistrationId == registrationMatch.Id)
                                                                 .Select(rsp => new ResponseMatch { Question = rsp.Title, Response = rsp.ResponseString })
                                                                 .ToList();

                }
                return req.CreateResponse(HttpStatusCode.OK, results);
            }
        }
    }

    public class RegistrationMatch
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<ResponseMatch> Responses { get; set; }
    }

    public class ResponseMatch
    {
        public string Question { get; set; }
        public string Response { get; set; }
    }
}

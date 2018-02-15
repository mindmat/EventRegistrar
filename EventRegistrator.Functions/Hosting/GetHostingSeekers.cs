using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Hosting
{
    public static class GetHostingSeekers
    {
        [FunctionName("GetHostingSeekers")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hostingseekers")]
            HttpRequestMessage req, TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrableIdHostingSeekers = Guid.Parse("EE10CE23-8219-44DF-9F2A-4FDEC3DE1867");
                var questionIdsPartners = new HashSet<Guid> { Guid.Parse("5374D089-2CD7-407B-B949-A0AB7FDBEB1A"), Guid.Parse("2B55BA53-F927-4CF1-8412-CA7B99D29137") };
                var questionIdsTravel = new HashSet<Guid> { Guid.Parse("FA2D18EA-BCC6-4BF2-9AA8-F551AE9EA5DA"), Guid.Parse("468B6D81-B350-430C-A390-931A3A4A1D04") };
                var questionIdsComment = new HashSet<Guid> { Guid.Parse("4B785896-19FE-494D-8839-80141138B62B"), Guid.Parse("AA2236CA-5E7A-4F4C-87C4-46031A7CC9FA") };
                var questionIdsPhone = new HashSet<Guid> { Guid.Parse("E4D2134C-3715-480F-9AA5-3FA48B8DDE09"), Guid.Parse("F7438124-61EF-4593-B2A1-6F02101462FA") };

                var responsesPartners = await dbContext.Responses
                                                      .Where(rsp => rsp.QuestionId.HasValue && questionIdsPartners.Contains(rsp.QuestionId.Value))
                                                      .ToListAsync();

                var responsesTravel = await dbContext.Responses
                                                         .Where(rsp => rsp.QuestionId.HasValue && questionIdsTravel.Contains(rsp.QuestionId.Value))
                                                         .ToListAsync();

                var responsesComment = await dbContext.Responses
                                                      .Where(rsp => rsp.QuestionId.HasValue && questionIdsComment.Contains(rsp.QuestionId.Value))
                                                      .ToListAsync();

                var responsesPhone = await dbContext.Responses
                                                    .Where(rsp => rsp.QuestionId.HasValue && questionIdsPhone.Contains(rsp.QuestionId.Value))
                                                    .ToListAsync();

                var seekers = await dbContext.Seats
                                             .Where(seat => seat.RegistrableId == registrableIdHostingSeekers &&
                                                            seat.Registration.IsWaitingList == false &&
                                                            seat.Registration.State != RegistrationState.Cancelled)
                                             .Select(seat => new
                                             {
                                                 seat.Registration.RespondentFirstName,
                                                 seat.Registration.RespondentLastName,
                                                 seat.Registration.RespondentEmail,
                                                 seat.RegistrationId,
                                                 seat.Registration.State,
                                                 seat.Registration.Language,
                                                 seat.Registration.AdmittedAt
                                             })
                                             .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, seekers
                                                             .Select(sek => new
                                                             {
                                                                 Id = sek.RegistrationId,
                                                                 FirstName = sek.RespondentFirstName,
                                                                 LastName = sek.RespondentLastName,
                                                                 Mail = sek.RespondentEmail,
                                                                 sek.Language,
                                                                 sek.State,
                                                                 sek.AdmittedAt,
                                                                 Partners = responsesPartners.FirstOrDefault(rsp => rsp.RegistrationId == sek.RegistrationId)?.ResponseString,
                                                                 Travel = responsesTravel.FirstOrDefault(rsp => rsp.RegistrationId == sek.RegistrationId)?.ResponseString,
                                                                 Comment = responsesComment.FirstOrDefault(rsp => rsp.RegistrationId == sek.RegistrationId)?.ResponseString,
                                                                 Phone = responsesPhone.FirstOrDefault(rsp => rsp.RegistrationId == sek.RegistrationId)?.ResponseString,
                                                             })
                                                             .OrderBy(sek => sek.AdmittedAt ?? DateTime.MaxValue));
            }
        }
    }
}

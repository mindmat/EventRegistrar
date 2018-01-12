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
    public static class GetHostingOffers
    {
        [FunctionName("GetHostingOffers")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hostingoffers")]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");


            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrableIdHostingOffer = Guid.Parse("1746CC39-7C95-4D03-B1BF-D599689B7B6A");
                var questionIdsAddress = new HashSet<Guid> { Guid.Parse("B8022360-94EE-4D17-AD00-0CDD713A69E5") };
                var questionIdsPlaceCount = new HashSet<Guid> { Guid.Parse("961002E7-6B6F-4B57-B34E-0887A7372711"), Guid.Parse("1009EDE1-AD0D-4671-9AB3-D2DB84C77506") };
                var questionIdsComment = new HashSet<Guid> { Guid.Parse("838322F5-0DB9-4560-9758-190F6A8E6FEF"), Guid.Parse("28D00FD4-675F-40B2-892E-9023D6A591DA") };
                var questionIdsPhone = new HashSet<Guid> { Guid.Parse("E4D2134C-3715-480F-9AA5-3FA48B8DDE09"), Guid.Parse("F7438124-61EF-4593-B2A1-6F02101462FA") };

                var responsesAddress = await dbContext.Responses
                                                      .Where(rsp => rsp.QuestionId.HasValue && questionIdsAddress.Contains(rsp.QuestionId.Value))
                                                      .ToListAsync();

                var responsesPlaceCount = await dbContext.Responses
                                                         .Where(rsp => rsp.QuestionId.HasValue && questionIdsPlaceCount.Contains(rsp.QuestionId.Value))
                                                         .ToListAsync();

                var responsesComment = await dbContext.Responses
                                                      .Where(rsp => rsp.QuestionId.HasValue && questionIdsComment.Contains(rsp.QuestionId.Value))
                                                      .ToListAsync();

                var responsesPhone = await dbContext.Responses
                                                    .Where(rsp => rsp.QuestionId.HasValue && questionIdsPhone.Contains(rsp.QuestionId.Value))
                                                    .ToListAsync();

                var offers = await dbContext.Seats
                                            .Where(seat => seat.RegistrableId == registrableIdHostingOffer &&
                                                           seat.Registration.IsWaitingList == false &&
                                                           seat.Registration.State != RegistrationState.Cancelled)
                                            .Select(seat => new
                                            {
                                                seat.Registration.RespondentFirstName,
                                                seat.Registration.RespondentLastName,
                                                seat.Registration.RespondentEmail,
                                                seat.RegistrationId,
                                                seat.Registration.State,
                                                seat.Registration.Language
                                            })
                                            .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, offers.Select(off => new
                {
                    Id = off.RegistrationId,
                    FirstName = off.RespondentFirstName,
                    LastName = off.RespondentLastName,
                    Mail = off.RespondentEmail,
                    off.Language,
                    off.State,
                    Address = responsesAddress.FirstOrDefault(rsp => rsp.RegistrationId == off.RegistrationId)?.ResponseString,
                    PlaceCount = responsesPlaceCount.FirstOrDefault(rsp => rsp.RegistrationId == off.RegistrationId)?.ResponseString,
                    Comment = responsesComment.FirstOrDefault(rsp => rsp.RegistrationId == off.RegistrationId)?.ResponseString,
                    Phone = responsesPhone.FirstOrDefault(rsp => rsp.RegistrationId == off.RegistrationId)?.ResponseString,
                }));
            }
        }
    }
}

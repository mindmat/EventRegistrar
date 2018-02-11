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

namespace EventRegistrator.Functions.Mailing
{
    public static class GetMailTemplates
    {
        [FunctionName("GetMailTemplates")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/mailTemplates")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var templates = await dbContext.MailTemplates
                                               .Where(mtp => mtp.Type == 0
                                                          && mtp.MailingKey != null
                                                          && mtp.EventId == eventId)
                                                          .Select(mtp => new MailTemplateDto
                                                          {
                                                              Key = mtp.MailingKey,
                                                              Language = mtp.Language,
                                                              Template = mtp.Template,
                                                              SenderMail = mtp.SenderMail,
                                                              SenderName = mtp.SenderName,
                                                              Subject = mtp.Subject
                                                          })
                                               .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, templates);
            }
        }
    }
}

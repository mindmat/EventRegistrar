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
    public static class GetMailTemplate
    {
        [FunctionName("GetMailTemplate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/mailTemplates/{mailingKey}")]
            HttpRequestMessage req,
            string eventIdString,
            string mailingKey,
            TraceWriter log)
        {
            log.Info("GetMailTemplate");

            var language = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "lang", StringComparison.OrdinalIgnoreCase) == 0)
                .Value?.ToLowerInvariant();

            if (language == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "no language provided");
            }
            var eventId = Guid.Parse(eventIdString);
            mailingKey = mailingKey?.ToLowerInvariant();
            log.Info($"key {mailingKey}, lang {language}");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var template = await dbContext.MailTemplates.FirstOrDefaultAsync(mtp => mtp.Type == 0
                                                                                     && mtp.Language == language
                                                                                     && mtp.MailingKey == mailingKey
                                                                                     && mtp.EventId == eventId);
                if (template == null)
                {
                    return req.CreateResponse(HttpStatusCode.OK, new MailTemplateDto
                    {
                        Language = language,
                        SenderMail = "info@leapinlindy.ch",
                        SenderName = "Leapin' Lindy",
                    });
                }
                var dto = new MailTemplateDto
                {
                    Language = template.Language,
                    Template = template.Template,
                    SenderMail = template.SenderMail,
                    SenderName = template.SenderName,
                    Subject = template.Subject
                };

                return req.CreateResponse(HttpStatusCode.OK, dto);
            }
        }
    }
}

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
    public static class SaveMailTemplate
    {
        [FunctionName("SaveMailTemplate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventIdString:guid}/mailTemplates/{mailingKey}")]
            HttpRequestMessage req,
            string eventIdString,
            string mailingKey,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);
            var dto = await req.Content.ReadAsAsync<MailTemplateDto>();
            mailingKey = GetMailTemplate.Normalize(mailingKey);

            if (dto.Language == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "no language provided");
            }
            if (dto.Template == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "no template provided");
            }
            if (dto.Subject == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "no subject provided");
            }
            if (dto.SenderMail == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "no sender provided");
            }

            dto.Language = dto.Language.ToLowerInvariant();
            using (var dbContext = new EventRegistratorDbContext())
            {
                var template = await dbContext.MailTemplates.FirstOrDefaultAsync(mtp => mtp.Type == 0
                                                                                     && mtp.Language == dto.Language
                                                                                     && mtp.MailingKey == mailingKey
                                                                                     && mtp.EventId == eventId);
                if (template == null)
                {
                    template = new MailTemplate
                    {
                        Id = Guid.NewGuid(),
                        Language = dto.Language,
                        ContentType = ContentType.Html,
                        EventId = eventId,
                        Type = 0,
                        MailingKey = mailingKey
                    };
                    dbContext.MailTemplates.Add(template);
                }
                template.Template = dto.Template;
                template.SenderMail = dto.SenderMail;
                template.SenderName = dto.SenderName;
                template.Subject = dto.Subject;
                template.MailingAudience = dto.Audience;

                await dbContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}

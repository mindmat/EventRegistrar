using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.WaitingList
{
    public static class SendOptionsToPersonsOnWaitingListHttpHandler
    {
        [FunctionName("SendOptionsToRegistrationsOnWaitingListHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event/{eventIdString:guid}/SendOptionsToRegistrationsOnWaitingList")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            if (!Guid.TryParse(eventIdString, out var eventId))
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{eventIdString} is not a guid");
            }

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrationsOnWaitingList = await dbContext.Registrations
                                                                .Where(reg => reg.RegistrationForm.EventId == eventId && 
                                                                              reg.IsWaitingList == true &&
                                                                              reg.State != RegistrationState.Cancelled)
                                                                .ToListAsync();

                log.Info($"registrations on waiting list: {registrationsOnWaitingList.Count}");

                const MailType mailType = MailType.OptionsForRegistrationsOnWaitingList;

                foreach (var registration in registrationsOnWaitingList)
                {
                    var templates = await dbContext.MailTemplates
                                                   .Where(mtp => mtp.EventId == eventId &&
                                                                 mtp.Type == mailType)
                                                   .ToListAsync();
                    if (!templates.Any())
                    {
                        throw new ArgumentException($"No template in event {eventId} with type {mailType}");
                    }
                    var language = registration.Language ?? ComposeAndSendMailCommandHandler.FallbackLanguage;
                    var template = templates.FirstOrDefault(mtp => mtp.Language == language) ??
                                   templates.FirstOrDefault(mtp => mtp.Language == ComposeAndSendMailCommandHandler.FallbackLanguage) ??
                                   templates.First();

                    var content = template.Template.Replace("{{FirstName}}", registration.RespondentFirstName);

                    var mail = new Mail
                    {
                        Id = Guid.NewGuid(),
                        Type = mailType,
                        SenderMail = template.SenderMail,
                        SenderName = template.SenderName,
                        Subject = template.Subject,
                        Recipients = registration.RespondentEmail,
                        Withhold = true,
                        Created = DateTime.UtcNow
                    };

                    if (template.ContentType == ContentType.Html)
                    {
                        mail.ContentHtml = content;
                    }
                    else
                    {
                        mail.ContentPlainText = content;
                    }

                    dbContext.Mails.Add(mail);
                    dbContext.MailToRegistrations.Add(new MailToRegistration { Id = Guid.NewGuid(), MailId = mail.Id, RegistrationId = registration.Id });
                }
                await dbContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK, $"{registrationsOnWaitingList.Count} Mails prepared");
            }
        }
    }
}

using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Sms;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Mailing
{
    public static class StartMailing
    {
        [FunctionName("StartMailing")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mailTemplates/{keyString}/send")]
            HttpRequestMessage req,
            string keyString,
            TraceWriter log)
        {
            var key = GetMailTemplate.Normalize(keyString);
            using (var dbContext = new EventRegistratorDbContext())
            {
                var templates = await dbContext.MailTemplates
                                               .Where(mtp => mtp.MailingKey == key
                                                          && mtp.Type == 0)
                                               .ToListAsync();

                foreach (var mailTemplate in templates)
                {
                    if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Paid) == true)
                    {
                        var registrations = await dbContext.Registrations
                                                           .Where(reg => reg.State == RegistrationState.Paid
                                                                      && reg.RegistrationForm.Language == mailTemplate.Language
                                                                      && !reg.Mails.Any(mail => mail.Mail.MailingKey == mailTemplate.MailingKey))
                                                           .ToListAsync();
                        log.Info($"paid {registrations.Count}");
                        foreach (var registration in registrations)
                        {
                            await CreateMail(dbContext, mailTemplate, registration);
                        }
                    }
                    if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Unpaid) == true)
                    {
                        var registrations = await dbContext.Registrations
                                                           .Where(reg => reg.State == RegistrationState.Received
                                                                      && reg.IsWaitingList != true
                                                                      && reg.RegistrationForm.Language == mailTemplate.Language
                                                                      && !reg.Mails.Any(mail => mail.Mail.MailingKey == mailTemplate.MailingKey))
                                                           .ToListAsync();
                        log.Info($"unpaid {registrations.Count}");
                        foreach (var registration in registrations)
                        {
                            await CreateMail(dbContext, mailTemplate, registration);
                        }
                    }
                    if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.WaitingList) == true)
                    {
                        var registrations = await dbContext.Registrations
                                                           .Where(reg => reg.State == RegistrationState.Received
                                                                      && reg.IsWaitingList == true
                                                                      && reg.RegistrationForm.Language == mailTemplate.Language
                                                                      && !reg.Mails.Any(mail => mail.Mail.MailingKey == mailTemplate.MailingKey))
                                                           .ToListAsync();
                        log.Info($"waiting list {registrations.Count}");
                        foreach (var registration in registrations)
                        {
                            await CreateMail(dbContext, mailTemplate, registration);
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task CreateMail(EventRegistratorDbContext dbContext, MailTemplate mailTemplate, Registration registration)
        {
            var mail = new Mail
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Recipients = registration.RespondentEmail,
                SenderMail = mailTemplate.SenderMail,
                SenderName = mailTemplate.SenderName,
                Subject = mailTemplate.Subject,
                Withhold = true,
                MailingKey = mailTemplate.MailingKey,
                ContentHtml = await new TemplateParameterFinder().Fill(mailTemplate.Template, registration.Id),
            };
            dbContext.Mails.Add(mail);
            dbContext.MailToRegistrations.Add(new MailToRegistration
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                MailId = mail.Id,
            });
        }

    }
}

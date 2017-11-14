using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Mailing
{
    public static class ReleaseWithheldMails
    {
        [FunctionName("ReleaseWithheldMails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event/{eventIdString}/ReleaseWithheldMails")]
               HttpRequestMessage req,
               string eventIdString,
               TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);
            // ToDo: Mails have to be assigned to events...
            using (var dbContext = new EventRegistratorDbContext())
            {
                var withheldMails = await dbContext.Mails
                                                   .Where(mail => mail.Withhold)
                                                   .Select(mail => new
                                                   {
                                                       Mail = mail,
                                                       To = mail.Registrations.Select(reg => new { reg.Registration.RespondentEmail, reg.Registration.RespondentFirstName })
                                                   })
                                                   .ToListAsync();
                log.Info($"Sending {withheldMails.Count} mails");
                foreach (var withheldMail in withheldMails)
                {
                    var sendMailCommand = new SendMailCommand
                    {
                        MailId = withheldMail.Mail.Id,
                        ContentHtml = withheldMail.Mail.ContentHtml,
                        ContentPlainText = withheldMail.Mail.ContentPlainText,
                        Subject = withheldMail.Mail.Subject,
                        Sender = new EmailAddress { Email = withheldMail.Mail.SenderMail, Name = withheldMail.Mail.SenderName },
                        To = withheldMail.To.Select(reg => new EmailAddress { Email = reg.RespondentEmail, Name = reg.RespondentFirstName }).ToList()
                    };

                    withheldMail.Mail.Withhold = false;
                    dbContext.Mails.Attach(withheldMail.Mail);
                    await ServiceBusClient.SendEvent(sendMailCommand, SendMailCommandHandler.SendMailQueueName);
                }
                await dbContext.SaveChangesAsync();
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
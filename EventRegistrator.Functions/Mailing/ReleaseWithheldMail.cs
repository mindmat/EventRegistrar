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
    public static class ReleaseWithheldMail
    {
        [FunctionName("ReleaseWithheldMail")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mails/{mailIdString:guid}/release")]
               HttpRequestMessage req,
               string mailIdString,
               TraceWriter log)
        {
            var mailId = Guid.Parse(mailIdString);


            using (var dbContext = new EventRegistratorDbContext())
            {
                var withheldMail = await dbContext.Mails
                                                  .Where(mail => mail.Id == mailId)
                                                  .Select(mail => new
                                                  {
                                                      Mail = mail,
                                                      To = mail.Registrations.Select(reg => new { reg.Registration.RespondentEmail, reg.Registration.RespondentFirstName })
                                                  })
                                                  .FirstOrDefaultAsync();
                var sendMailCommand = new SendMailCommand
                {
                    MailId = withheldMail.Mail.Id,
                    ContentHtml = withheldMail.Mail.ContentHtml,
                    ContentPlainText = withheldMail.Mail.ContentPlainText,
                    Subject = withheldMail.Mail.Subject,
                    Sender = new EmailAddress { Email = withheldMail.Mail.SenderMail, Name = withheldMail.Mail.SenderName },
                    To = withheldMail.To.Select(reg => new EmailAddress { Email = reg.RespondentEmail, Name = reg.RespondentFirstName }).ToList()
                };

                dbContext.Mails.Attach(withheldMail.Mail);
                withheldMail.Mail.Withhold = false;

                await ServiceBusClient.SendEvent(sendMailCommand, SendMailCommandHandler.SendMailQueueName);

                await dbContext.SaveChangesAsync();
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
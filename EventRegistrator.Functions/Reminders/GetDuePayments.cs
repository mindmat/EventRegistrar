using System;
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

namespace EventRegistrator.Functions.Reminders
{
  public static class GetDuePayments
  {
    [FunctionName("GetDuePayments")]
    public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString}/duePayments")]
            HttpRequestMessage req,
        string eventIdString,
        TraceWriter log)
    {
      var eventId = Guid.Parse(eventIdString);

      using (var dbContext = new EventRegistratorDbContext())
      {
        var reminderDueFrom = DateTime.UtcNow.AddDays(-SendReminderCommandHandler.DefaultPaymentGracePeriod);
        var dueRegistrations = await dbContext.Registrations
                                              .Where(reg => reg.RegistrationForm.EventId == eventId &&
                                                            reg.State == RegistrationState.Received &&
                                                            reg.IsWaitingList != true)
                                              .Select(reg => new
                                              {
                                                reg.Id,
                                                FirstName = reg.RespondentFirstName,
                                                LastName = reg.RespondentLastName,
                                                Email = reg.RespondentEmail,
                                                reg.Price,
                                                reg.ReceivedAt,
                                                AcceptedMail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                      SendReminderCommandHandler.MailTypes_Accepted.Contains(map.Mail.Type))
                                                                        .Select(map => new
                                                                        {
                                                                          map.MailId,
                                                                          map.Mail.Created
                                                                        })
                                                                        .OrderByDescending(mail => mail.Created)
                                                                        .FirstOrDefault(),
                                                Reminder1Mail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                       SendReminderCommandHandler.MailTypes_Reminder1.Contains(map.Mail.Type))
                                                                         .Select(map => new
                                                                         {
                                                                           map.MailId,
                                                                           map.Mail.Created
                                                                         })
                                                                         .OrderByDescending(mail => mail.Created)
                                                                         .FirstOrDefault(),
                                                Reminder2Mail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                       SendReminderCommandHandler.MailTypes_Reminder2.Contains(map.Mail.Type))
                                                                         .Select(map => new
                                                                         {
                                                                           map.MailId,
                                                                           map.Mail.Created
                                                                         })
                                                                         .OrderByDescending(mail => mail.Created)
                                                                         .FirstOrDefault(),
                                                reg.ReminderLevel,
                                                Paid = (decimal?)reg.Payments.Sum(ass => ass.Amount)
                                              })
                                              .OrderBy(reg => reg.AcceptedMail.Created)
                                              .Select(reg => new
                                              {
                                                reg.Id,
                                                reg.FirstName,
                                                reg.LastName,
                                                reg.Email,
                                                reg.Price,
                                                reg.Paid,
                                                reg.ReceivedAt,
                                                reg.AcceptedMail,
                                                reg.Reminder1Mail,
                                                reg.Reminder2Mail,
                                                reg.ReminderLevel,
                                                Reminder1Due = reg.Reminder1Mail == null &&
                                                               reg.AcceptedMail != null &&
                                                               reg.AcceptedMail.Created < reminderDueFrom,
                                                Reminder2Due = reg.Reminder2Mail == null &&
                                                               reg.Reminder1Mail != null &&
                                                               reg.Reminder1Mail.Created < reminderDueFrom,
                                              })
                                              .ToListAsync();

        return req.CreateResponse(HttpStatusCode.OK, dueRegistrations);
      }
    }
  }
}
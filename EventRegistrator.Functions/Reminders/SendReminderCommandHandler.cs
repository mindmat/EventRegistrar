using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.Reminders
{
    public static class SendReminderCommandHandler
    {
        public const string SendReminderCommandsQueueName = "SendReminderCommands";
        public static readonly HashSet<MailType?> MailTypes_Accepted = new HashSet<MailType?> { MailType.DoubleRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };
        public static readonly HashSet<MailType?> MailTypes_Reminder1 = new HashSet<MailType?> { MailType.DoubleRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder };
        public static readonly HashSet<MailType?> MailTypes_Reminder2 = new HashSet<MailType?> { MailType.DoubleRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder };
        public const int DefaultPaymentGracePeriod = 6;

        public static bool IsPaymentDue(DateTime startOfGracePeriodUtc, int? paymentGracePeriod = null)
        {
            return (DateTime.UtcNow - startOfGracePeriodUtc).TotalDays > (paymentGracePeriod ?? DefaultPaymentGracePeriod);
        }

        public static bool IsMailTypeThatTriggerPaymentDeadline(MailType mailType)
        {
            return MailTypes_Accepted.Contains(mailType);
        }

        [FunctionName("SendReminderCommandHandler")]
        public static async Task Run([ServiceBusTrigger(SendReminderCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]
            SendReminderCommand command,
            TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var registration = await dbContext.Registrations
                                                  .Where(reg => reg.Id == command.RegistrationId)
                                                  .Select(reg => new
                                                  {
                                                      Registration = reg,
                                                      StartPaymentPeriodMail = reg.Mails.Where(map => MailTypes_Accepted.Contains(map.Mail.Type))
                                                                                        .Select(map => map.Mail)
                                                                                        .OrderByDescending(mail => mail.Created)
                                                                                        .FirstOrDefault()
                                                  })
                                                  .FirstOrDefaultAsync();
                if (registration == null)
                {
                    log.Info($"No registration found with Id {command.RegistrationId}");
                    return;
                }
                if (registration.Registration.IsWaitingList == true)
                {
                    log.Info($"Registration {command.RegistrationId} is on the waiting list and doesn't have to pay yet");
                    return;
                }
                if (registration.Registration.State == RegistrationState.Cancelled)
                {
                    log.Info($"Registration {command.RegistrationId} is cancelled, so no payment needed anymore");
                    return;
                }
                if (registration.Registration.State == RegistrationState.Paid)
                {
                    log.Info($"Registration {command.RegistrationId} is already paid");
                    return;
                }
                if (registration.StartPaymentPeriodMail == null)
                {
                    log.Info($"Registration {registration.Registration.Id} lacks the accepted mail");
                    return;
                }
                if (!IsPaymentDue(registration.StartPaymentPeriodMail.Created, DefaultPaymentGracePeriod))
                {
                    log.Info($"Registration {registration.Registration.Id} is not due yet");
                    return;
                }

                await ServiceBusClient.SendEvent(
                    new ComposeAndSendMailCommand
                    {
                        RegistrationId = registration.Registration.Id,
                        Withhold = command.Withhold,
                        AllowDuplicate = false
                    }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            }
        }
    }
}
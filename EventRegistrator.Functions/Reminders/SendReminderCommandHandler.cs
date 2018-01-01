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
        private static readonly HashSet<MailType?> MailTypesThatTriggerPaymentDeadline = new HashSet<MailType?> { MailType.DoubleRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };
        private const int DefaultPaymentGracePeriod = 14;

        public static bool IsPaymentDue(DateTime startOfGracePeriodUtc, int? paymentGracePeriod = null)
        {
            return (DateTime.UtcNow - startOfGracePeriodUtc).TotalDays > (paymentGracePeriod ?? DefaultPaymentGracePeriod);
        }

        public static bool IsMailTypeThatTriggerPaymentDeadline(MailType mailType)
        {
            return MailTypesThatTriggerPaymentDeadline.Contains(mailType);
        }

        [FunctionName("SendReminderCommandHandler")]
        public static async Task Run([ServiceBusTrigger(SendReminderCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]
            SendReminderCommand command, TraceWriter log)
        {
            //log.Info($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var gracePeriod = command.GracePeriodInDays ?? DefaultPaymentGracePeriod;

                var registrations = dbContext.Registrations
                                             .Where(reg => reg.State == RegistrationState.Received && 
                                                           reg.IsWaitingList == false)
                                             .Select(reg => new
                                             {
                                                 Registration = reg,
                                                 StartPaymentPeriodMail = reg.Mails.Where(map => MailTypesThatTriggerPaymentDeadline.Contains(map.Mail.Type))
                                                                                   .Select(map => map.Mail)
                                                                                   .OrderByDescending(mail => mail.Created)
                                                                                   .FirstOrDefault()
                                             });
                                             //.Where(tmp => (DateTime.UtcNow - tmp.StartPaymentPeriodMail.Created).Days > gracePeriod);
                if (command.RegistrationId.HasValue)
                {
                    registrations = registrations.Where(reg => reg.Registration.Id == command.RegistrationId);
                }
                var registrationsLocal = await registrations.ToListAsync();
                log.Info($"Due registrations count: {registrationsLocal.Count}");

                foreach (var dueRegistration in registrationsLocal)
                {
                    if (dueRegistration.StartPaymentPeriodMail == null)
                    {
                        log.Info($"Registration with id {dueRegistration.Registration.Id} has no accepted mail");
                        continue;
                    }
                    if (IsPaymentDue(dueRegistration.StartPaymentPeriodMail.Created, gracePeriod))
                    {
                        await ServiceBusClient.SendEvent(
                            new ComposeAndSendMailCommand
                            {
                                RegistrationId = dueRegistration.Registration.Id,
                                Withhold = true,
                                AllowDuplicate = false
                            }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
                    }

                }
            }
        }
    }
}
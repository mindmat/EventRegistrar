using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Payments.Due
{
    public class SendReminderCommandHandler : IRequestHandler<SendReminderCommand>
    {
        private readonly ILogger _logger;
        private readonly IQueryable<MailToRegistration> _mailsToRegistrations;
        private readonly DuePaymentConfiguration _paymentConfiguration;
        private readonly IRepository<Registration> _registrations;
        private readonly ServiceBusClient _serviceBusClient;

        public SendReminderCommandHandler(ILogger logger,
                                          IRepository<Registration> registrations,
                                          IQueryable<MailToRegistration> mailsToRegistrations,
                                          DuePaymentConfiguration paymentConfiguration,
                                          ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _registrations = registrations;
            _mailsToRegistrations = mailsToRegistrations;
            _paymentConfiguration = paymentConfiguration;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(SendReminderCommand command, CancellationToken cancellationToken)
        {
            var tmp = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                          .Select(reg => new
                                          {
                                              Registration = reg,
                                              StartPaymentPeriodMail = reg.Mails.Where(map => map.Mail.Type.HasValue && _paymentConfiguration.MailTypes_Accepted.Contains(map.Mail.Type.Value))
                                                                                .Select(map => map.Mail)
                                                                                .OrderByDescending(mail => mail.Created)
                                                                                .FirstOrDefault()
                                          })
                                          .FirstAsync(cancellationToken);

            var registration = tmp.Registration;

            if (registration.IsWaitingList == true)
            {
                _logger.LogInformation($"Registration {command.RegistrationId} is on the waiting list and doesn't have to pay yet");
                return Unit.Value;
            }
            if (registration.State == RegistrationState.Cancelled)
            {
                _logger.LogInformation($"Registration {command.RegistrationId} is cancelled, so no payment needed anymore");
                return Unit.Value;
            }
            if (registration.State == RegistrationState.Paid)
            {
                _logger.LogInformation($"Registration {command.RegistrationId} is already paid");
                return Unit.Value;
            }
            if (tmp.StartPaymentPeriodMail == null)
            {
                _logger.LogInformation($"Registration {registration.Id} lacks the accepted mail");
                return Unit.Value;
            }
            if (!IsPaymentDue(tmp.StartPaymentPeriodMail.Created, _paymentConfiguration.PaymentGracePeriod))
            {
                _logger.LogInformation($"Registration {registration.Id} is not due yet");
                return Unit.Value;
            }

            var acceptedMail = await _mailsToRegistrations
                                     .Where(map => map.RegistrationId == command.RegistrationId && map.Mail.Type.HasValue && _paymentConfiguration.MailTypes_Accepted.Contains(map.Mail.Type.Value))
                                     .OrderByDescending(map => map.Mail.Created)
                                     .Include(map => map.Mail)
                                     .FirstOrDefaultAsync(cancellationToken);
            if (acceptedMail != null)
            {
                var acceptedDate = acceptedMail.Mail.Created;
                _logger.LogInformation($"acceptedDate {acceptedDate}, mailid {acceptedMail.Mail.Id}");
                if (IsPaymentDue(acceptedDate))
                {
                    // payment is overdue, check reminder level
                    var newLevel = registration.ReminderLevel + 1;
                    MailType? mailType = null;
                    if (newLevel == 1)
                    {
                        mailType = registration.RegistrationId_Partner.HasValue
                            ? MailType.PartnerRegistrationFirstReminder
                            : MailType.SingleRegistrationFirstReminder;
                        registration.ReminderLevel = newLevel;
                    }
                    else if (newLevel == 2)
                    {
                        mailType = registration.RegistrationId_Partner.HasValue
                          ? MailType.PartnerRegistrationSecondReminder
                          : MailType.SingleRegistrationSecondReminder;
                        registration.ReminderLevel = newLevel;
                    }

                    await _registrations.InsertOrUpdateEntity(registration, cancellationToken);

                    if (mailType != null)
                    {
                        _serviceBusClient.SendMessage(new ComposeAndSendMailCommand
                        {
                            MailType = mailType.Value,
                            RegistrationId = registration.Id,
                            Withhold = true,
                            AllowDuplicate = false
                        });
                    }
                }
            }
            else
            {
                _logger.LogInformation("unexpected situation: no accepted mail found");
            }

            return Unit.Value;
        }

        public bool IsPaymentDue(DateTime startOfGracePeriodUtc, int? paymentGracePeriod = null)
        {
            return (DateTime.UtcNow - startOfGracePeriodUtc).TotalDays > (paymentGracePeriod ?? _paymentConfiguration.PaymentGracePeriod);
        }
    }
}
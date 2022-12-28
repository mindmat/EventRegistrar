using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Due;

public class SendReminderMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool Withhold { get; set; }
}

public class SendReminderMailCommandHandler : AsyncRequestHandler<SendReminderMailCommand>
{
    private readonly ILogger _logger;
    private readonly IQueryable<MailToRegistration> _mailsToRegistrations;
    private readonly DuePaymentConfiguration _paymentConfiguration;
    private readonly IRepository<Registration> _registrations;
    private readonly CommandQueue _commandQueue;

    public SendReminderMailCommandHandler(ILogger logger,
                                          IRepository<Registration> registrations,
                                          IQueryable<MailToRegistration> mailsToRegistrations,
                                          DuePaymentConfiguration paymentConfiguration,
                                          CommandQueue commandQueue)
    {
        _logger = logger;
        _registrations = registrations;
        _mailsToRegistrations = mailsToRegistrations;
        _paymentConfiguration = paymentConfiguration;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(SendReminderMailCommand command, CancellationToken cancellationToken)
    {
        var tmp = await _registrations.Where(reg => reg.Id == command.RegistrationId
                                                 && reg.EventId == command.EventId)
                                      .Select(reg => new
                                                     {
                                                         Registration = reg,
                                                         StartPaymentPeriod = reg.Mails!
                                                                                 .Where(map => _paymentConfiguration.MailTypes_Accepted.Contains(map.Mail!.Type!.Value))
                                                                                 .Select(map => map.Mail)
                                                                                 .Max(mail => mail!.Created)
                                                     })
                                      .FirstAsync(cancellationToken);

        var registration = tmp.Registration;

        if (registration.IsOnWaitingList == true)
        {
            _logger.LogInformation($"Registration {command.RegistrationId} is on the waiting list and doesn't have to pay yet");
            return;
        }

        if (registration.State == RegistrationState.Cancelled)
        {
            _logger.LogInformation($"Registration {command.RegistrationId} is cancelled, so no payment needed anymore");
            return;
        }

        if (registration.State == RegistrationState.Paid)
        {
            _logger.LogInformation($"Registration {command.RegistrationId} is already paid");
            return;
        }

        if (tmp.StartPaymentPeriod == default)
        {
            _logger.LogInformation($"Registration {registration.Id} lacks the accepted mail");
            return;
        }

        if (!IsPaymentDue(tmp.StartPaymentPeriod, _paymentConfiguration.PaymentGracePeriod))
        {
            _logger.LogInformation($"Registration {registration.Id} is not due yet");
            return;
        }

        var acceptedMail = await _mailsToRegistrations.Where(map => map.RegistrationId == command.RegistrationId
                                                                 && map.Mail!.Type != null
                                                                 && _paymentConfiguration.MailTypes_Accepted.Contains(map.Mail.Type.Value))
                                                      .Include(map => map.Mail)
                                                      .OrderByDescending(map => map.Mail!.Created)
                                                      .FirstOrDefaultAsync(cancellationToken);
        if (acceptedMail != null)
        {
            var acceptedDate = acceptedMail.Mail!.Created;
            _logger.LogInformation($"acceptedDate {acceptedDate}, mailid {acceptedMail.Mail.Id}");
            if (IsPaymentDue(acceptedDate))
            {
                // payment is overdue, check reminder level
                var newLevel = registration.ReminderLevel + 1;
                MailType? mailType = null;
                if (newLevel == 1)
                {
                    mailType = registration.RegistrationId_Partner != null
                                   ? MailType.PartnerRegistrationFirstReminder
                                   : MailType.SingleRegistrationFirstReminder;
                    registration.ReminderLevel = newLevel;
                }
                else if (newLevel == 2)
                {
                    mailType = registration.RegistrationId_Partner != null
                                   ? MailType.PartnerRegistrationSecondReminder
                                   : MailType.SingleRegistrationSecondReminder;
                    registration.ReminderLevel = newLevel;
                }

                await _registrations.InsertOrUpdateEntity(registration, cancellationToken);

                if (mailType != null)
                {
                    _commandQueue.EnqueueCommand(new ComposeAndSendAutoMailCommand
                                                 {
                                                     EventId = command.EventId,
                                                     MailType = mailType.Value,
                                                     RegistrationId = registration.Id,
                                                     AllowDuplicate = false
                                                 });
                }
            }
        }
        else
        {
            _logger.LogInformation("unexpected situation: no accepted mail found");
        }
    }

    public bool IsPaymentDue(DateTimeOffset startOfGracePeriodUtc, int? paymentGracePeriod = null)
    {
        return (DateTime.UtcNow - startOfGracePeriodUtc).TotalDays > (paymentGracePeriod ?? _paymentConfiguration.PaymentGracePeriod);
    }
}
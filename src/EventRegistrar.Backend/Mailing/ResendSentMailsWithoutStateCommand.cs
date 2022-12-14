using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing;

public class ResendSentMailsWithoutStateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ResendSentMailsWithoutStateCommandHandler : AsyncRequestHandler<ResendSentMailsWithoutStateCommand>
{
    private readonly IQueryable<Mail> _mails;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CommandQueue _commandQueue;

    public ResendSentMailsWithoutStateCommandHandler(IQueryable<Mail> mails,
                                                     IDateTimeProvider dateTimeProvider,
                                                     CommandQueue commandQueue)
    {
        _mails = mails;
        _dateTimeProvider = dateTimeProvider;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(ResendSentMailsWithoutStateCommand command, CancellationToken cancellationToken)
    {
        var threshold = _dateTimeProvider.Now.AddMinutes(-10);

        var failedMailIds = await _mails.Where(mail => mail.EventId == command.EventId
                                                    && mail.Sent < threshold
                                                    && mail.SendGridMessageId == null
                                                    && mail.State == null
                                                    && !mail.Discarded
                                                    && !mail.Withhold
                                                    && !mail.Registrations!.Any(reg => reg.Registration!.State == RegistrationState.Cancelled))
                                        .OrderBy(mail => mail.Type)
                                        .Select(mail => mail.Id)
                                        .ToListAsync(cancellationToken);

        foreach (var failedMailId in failedMailIds)
        {
            _commandQueue.EnqueueCommand(new ReleaseMailCommand { EventId = command.EventId, MailId = failedMailId });
        }
    }
}
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing;

public class ResendSentMailsWithoutStateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ResendSentMailsWithoutStateCommandHandler(IQueryable<Mail> mails,
                                                       IDateTimeProvider dateTimeProvider,
                                                       CommandQueue commandQueue)
    : IRequestHandler<ResendSentMailsWithoutStateCommand>
{
    public async Task Handle(ResendSentMailsWithoutStateCommand command, CancellationToken cancellationToken)
    {
        var threshold = dateTimeProvider.Now.AddMinutes(-10);

        var failedMailIds = await mails.Where(mail => mail.EventId == command.EventId
                                                   && mail.Sent < threshold
                                                   && mail.SendGridMessageId == null
                                                   && mail.State == null
                                                   && !mail.Discarded
                                                   && !mail.Withhold
                                                   && !mail.Registrations!.Any(reg => reg.Registration!.State == RegistrationState.Cancelled))
                                       .OrderBy(mail => mail.Type)
                                       .Select(mail => mail.Id)
                                       .ToListAsync(cancellationToken);

        commandQueue.EnqueueCommand(new ReleaseMailsCommand
                                    {
                                        EventId = command.EventId,
                                        MailIds = failedMailIds
                                    });
    }
}
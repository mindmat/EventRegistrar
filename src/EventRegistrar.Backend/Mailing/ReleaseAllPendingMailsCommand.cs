using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Mailing;

public class ReleaseAllPendingMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ReleaseAllPendingMailsCommandHandler : AsyncRequestHandler<ReleaseAllPendingMailsCommand>
{
    private readonly IRepository<Mail> _mails;
    private readonly CommandQueue _commandQueue;

    public ReleaseAllPendingMailsCommandHandler(IRepository<Mail> mails,
                                                CommandQueue commandQueue)
    {
        _mails = mails;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(ReleaseAllPendingMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await _mails.Where(mail => mail.EventId == command.EventId
                                                    && !mail.Discarded
                                                    && mail.State == null
                                                    && mail.Sent == null)
                                        .Include(mail => mail.Registrations!)
                                        .ThenInclude(map => map.Registration)
                                        .OrderByDescending(mail => mail.Created)
                                        .ToListAsync(cancellationToken);
        foreach (var withheldMail in withheldMails)
        {
            _commandQueue.EnqueueCommand(new ReleaseMailsCommand
                                         {
                                             EventId = command.EventId,
                                             MailIds = new[] { withheldMail.Id }
                                         });
        }
    }
}
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Mailing;

public class ReleaseAllPendingMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ReleaseAllPendingMailsCommandHandler(IRepository<Mail> mails,
                                                  ChangeTrigger changeTrigger)
    : IRequestHandler<ReleaseAllPendingMailsCommand>
{
    public async Task Handle(ReleaseAllPendingMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await mails.Where(mail => mail.EventId == command.EventId
                                                   && !mail.Discarded
                                                   && mail.State == null
                                                   && mail.Sent == null)
                                       .Include(mail => mail.Registrations!)
                                       .ThenInclude(map => map.Registration)
                                       .OrderByDescending(mail => mail.Created)
                                       .ToListAsync(cancellationToken);
        foreach (var withheldMail in withheldMails)
        {
            changeTrigger.EnqueueCommand(new ReleaseMailsCommand
                                         {
                                             EventId = command.EventId,
                                             MailIds = new[] { withheldMail.Id }
                                         });
        }
    }
}
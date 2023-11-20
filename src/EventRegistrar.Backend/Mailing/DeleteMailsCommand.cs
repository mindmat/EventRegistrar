using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Mailing;

public class DeleteMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<Guid> MailIds { get; set; } = null!;
}

public class DeleteMailsCommandHandler(IRepository<Mail> mails,
                                       ChangeTrigger changeTrigger,
                                       IEventBus eventBus)
    : IRequestHandler<DeleteMailsCommand>
{
    public async Task Handle(DeleteMailsCommand command, CancellationToken cancellationToken)
    {
        var mailsToDelete = await mails.Where(mail => command.MailIds.Contains(mail.Id)
                                                   && mail.EventId == command.EventId)
                                       .Include(mail => mail.Registrations)
                                       .ToListAsync(cancellationToken);
        foreach (var mailToDelete in mailsToDelete)
        {
            mailToDelete.Discarded = true;
            foreach (var registrationId in mailToDelete.Registrations!.Select(reg => reg.RegistrationId))
            {
                changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId, command.EventId);
            }
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PendingMailsQuery)
                         });
    }
}
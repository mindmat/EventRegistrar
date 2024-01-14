using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing;

public class ReleaseMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<Guid> MailIds { get; set; } = null!;
}

public class ReleaseMailsCommandHandler(IRepository<Mail> mails,
                                        CommandQueue commandQueue,
                                        IEventBus eventBus,
                                        IDateTimeProvider dateTimeProvider,
                                        ChangeTrigger changeTrigger)
    : IRequestHandler<ReleaseMailsCommand>
{
    public async Task Handle(ReleaseMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await mails.Where(mail => command.MailIds.Contains(mail.Id)
                                                   && !mail.Discarded)
                                       .Include(mail => mail.Registrations!)
                                       .ThenInclude(map => map.Registration)
                                       .ToListAsync(cancellationToken);

        foreach (var withheldMail in withheldMails)
        {
            var sendMailCommand = new SendMailCommand
                                  {
                                      EventId = withheldMail.EventId!.Value,
                                      MailId = withheldMail.Id
                                  };

            withheldMail.Withhold = false;
            withheldMail.Sent = dateTimeProvider.Now;

            commandQueue.EnqueueCommand(sendMailCommand);

            eventBus.Publish(new MailReleased
                             {
                                 MailId = withheldMail.Id,
                                 To = withheldMail.Recipients,
                                 Subject = withheldMail.Subject
                             });

            withheldMail.Registrations!.ForEach(reg => changeTrigger.TriggerUpdate<RegistrationCalculator>(reg.RegistrationId, command.EventId));
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PendingMailsQuery)
                         });
    }
}
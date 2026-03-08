using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class ReleaseBulkMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string BulkMailKey { get; set; } = null!;
}

public class ReleaseBulkMailsCommandHandler(IRepository<Mail> mails,
                                            ChangeTrigger changeTrigger,
                                            IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ReleaseBulkMailsCommand>
{
    private const int ChunkSize = 100;

    public async Task Handle(ReleaseBulkMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await mails.AsTracking()
                                       .Where(mail => mail.BulkMailKey == command.BulkMailKey
                                                   && mail.EventId == command.EventId
                                                   && mail.Withhold
                                                   && !mail.Discarded)
                                       .Include(mail => mail.Registrations!)
                                       .ThenInclude(map => map.Registration)
                                       .Take(ChunkSize)
                                       .ToListAsync(cancellationToken);

        foreach (var withheldMail in withheldMails)
        {
            var sendMailCommand = new SendMailCommand
                                  {
                                      EventId = command.EventId,
                                      MailId = withheldMail.Id
                                  };

            withheldMail.Withhold = false;
            withheldMail.Sent = dateTimeProvider.Now;

            changeTrigger.EnqueueCommand(sendMailCommand);
            foreach (var registrationId in withheldMail.Registrations?.Select(rgm => rgm.RegistrationId) ?? [])
            {
                changeTrigger.TriggerUpdate<RegistrationCalculator>(command.EventId, registrationId);
            }
        }

        changeTrigger.QueryChanged<GeneratedBulkMailsQuery>(command.EventId);
        changeTrigger.TriggerUpdate<PendingMailsCalculator>(command.EventId);
        if (withheldMails.Count >= ChunkSize)
        {
            // enqueue next chunk
            changeTrigger.EnqueueCommand(new ReleaseBulkMailsCommand
                                         {
                                             EventId = command.EventId,
                                             BulkMailKey = command.BulkMailKey
                                         });
        }
    }
}
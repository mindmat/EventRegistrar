using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class CreateBulkMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? BulkMailKey { get; set; }
}

public class CreateBulkMailsCommandHandler(IQueryable<BulkMailTemplate> mailTemplates,
                                           IQueryable<Registration> registrations,
                                           IQueryable<Event> events,
                                           IRepository<Mail> mails,
                                           IRepository<MailToRegistration> mailsToRegistrations,
                                           MailConfiguration configuration,
                                           MailComposer mailComposer,
                                           IDateTimeProvider dateTimeProvider,
                                           IEventBus eventBus,
                                           CommandQueue commandQueue)
    : IRequestHandler<CreateBulkMailsCommand>
{
    private const int ChunkSize = 100;

    public async Task Handle(CreateBulkMailsCommand command, CancellationToken cancellationToken)
    {
        var templates = await mailTemplates.Where(mtp => mtp.EventId == command.EventId
                                                      && mtp.BulkMailKey == command.BulkMailKey)
                                           .ToListAsync(cancellationToken);

        var registrationsOfEvent = await registrations.Where(reg => reg.EventId == command.EventId
                                                                 && reg.State != RegistrationState.Cancelled
                                                                 && !reg.Mails!.Any(mail => mail.Mail!.BulkMailKey == command.BulkMailKey))
                                                      .Include(reg => reg.Seats_AsLeader)
                                                      .Include(reg => reg.Seats_AsFollower)
                                                      .ToListAsync(cancellationToken);
        var remainingChunkSize = ChunkSize;
        foreach (var mailTemplate in templates)
        {
            if (remainingChunkSize <= 0)
            {
                break;
            }

            var registrationsForTemplate = mailTemplate.RegistrableId == null
                                               ? registrationsOfEvent
                                               : registrationsOfEvent.Where(reg => reg.Seats_AsLeader!.Any(spt => !spt.IsCancelled
                                                                                                               && spt.RegistrableId == mailTemplate.RegistrableId)
                                                                                || reg.Seats_AsFollower!.Any(spt => !spt.IsCancelled
                                                                                                                 && spt.RegistrableId == mailTemplate.RegistrableId))
                                                                     .Take(ChunkSize)
                                                                     .ToList();
            var receivers = new List<Registration>();
            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Paid) == true)
            {
                receivers.AddRange(registrationsForTemplate.Where(reg => reg.State == RegistrationState.Paid
                                                                      && (reg.Language == mailTemplate.Language || reg.Language == null)));
            }

            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Unpaid) == true)
            {
                receivers.AddRange(registrationsForTemplate.Where(reg => reg.State == RegistrationState.Received
                                                                      && (reg.Language == mailTemplate.Language || reg.Language == null)
                                                                      && reg.IsOnWaitingList != true));
            }

            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.WaitingList) == true)
            {
                receivers.AddRange(registrationsForTemplate.Where(reg => reg.State == RegistrationState.Received
                                                                      && (reg.Language == mailTemplate.Language || reg.Language == null)
                                                                      && reg.IsOnWaitingList == true));
            }

            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.PredecessorEvent) == true)
            {
                var alreadyCoveredMailAddresses = await mails.Where(mail => mail.BulkMailKey == command.BulkMailKey)
                                                             .SelectMany(mail => mail.Registrations!)
                                                             .Select(map => map.Registration!.RespondentEmail)
                                                             .ToListAsync(cancellationToken);

                // distinct by email, not registration id
                var predecessorReceivers = await events.Where(evt => evt.Id == command.EventId)
                                                       .SelectMany(evt => evt.PredecessorEvent!.Registrations!)
                                                       .Where(reg => !alreadyCoveredMailAddresses.Contains(reg.RespondentEmail))
                                                       .Take(ChunkSize)
                                                       .ToListAsync(cancellationToken);
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.PrePredecessorEvent) == true)
                {
                    predecessorReceivers.AddRange(await events.Where(evt => evt.Id == command.EventId)
                                                              .SelectMany(evt => evt.PredecessorEvent!.PredecessorEvent!.Registrations!)
                                                              .Where(reg => !alreadyCoveredMailAddresses.Contains(reg.RespondentEmail))
                                                              .Take(ChunkSize)
                                                              .ToListAsync(cancellationToken));
                }

                receivers.AddRange(predecessorReceivers.DistinctBy(reg => reg.RespondentEmail!.ToLower()).Take(ChunkSize));
            }

            receivers = receivers.DistinctBy(reg => reg.Id)
                                 .Take(remainingChunkSize)
                                 .ToList();
            foreach (var registration in receivers)
            {
                await CreateMail(mailTemplate, registration, cancellationToken);
            }

            remainingChunkSize -= receivers.Count;
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(GeneratedBulkMailsQuery)
                         });
        if (remainingChunkSize <= 0)
        {
            // enqueue next chunk
            commandQueue.EnqueueCommand(new CreateBulkMailsCommand
                                        {
                                            EventId = command.EventId,
                                            BulkMailKey = command.BulkMailKey
                                        });
        }
    }

    private async Task CreateMail(BulkMailTemplate mailTemplate,
                                  Registration registration,
                                  CancellationToken cancellationToken)
    {
        var content = await mailComposer.Compose(registration.Id,
                                                 mailTemplate.ContentHtml,
                                                 mailTemplate.Language,
                                                 cancellationToken);
        var mail = new Mail
                   {
                       Id = Guid.NewGuid(),
                       Created = dateTimeProvider.Now,
                       Recipients = registration.RespondentEmail,
                       SenderMail = mailTemplate.SenderMail ?? configuration.SenderMail,
                       SenderName = mailTemplate.SenderName ?? configuration.SenderName,
                       Subject = mailTemplate.Subject,
                       Withhold = true,
                       BulkMailKey = mailTemplate.BulkMailKey,
                       ContentHtml = content,
                       EventId = mailTemplate.EventId
                       //MailTemplateId = mailTemplate.Id //ToDo
                   };
        mails.InsertObjectTree(mail);
        mailsToRegistrations.InsertObjectTree(new MailToRegistration
                                              {
                                                  Id = Guid.NewGuid(),
                                                  RegistrationId = registration.Id,
                                                  Email = registration.RespondentEmail?.ToLowerInvariant(),
                                                  MailId = mail.Id
                                              });
    }
}
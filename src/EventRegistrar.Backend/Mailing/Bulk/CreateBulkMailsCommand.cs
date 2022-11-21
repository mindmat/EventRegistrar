using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class CreateBulkMailsCommand : IRequest, IEventBoundRequest
{
    public string BulkMailKey { get; set; }
    public Guid EventId { get; set; }
}

public class CreateBulkMailsCommandHandler : IRequestHandler<CreateBulkMailsCommand>
{
    private readonly MailComposer _mailComposer;
    private readonly IRepository<Mail> _mails;
    private readonly IRepository<MailToRegistration> _mailsToRegistrations;
    private readonly IQueryable<MailTemplate> _mailTemplates;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Event> _events;
    private const int ChunkSize = 100;

    public CreateBulkMailsCommandHandler(IQueryable<MailTemplate> mailTemplates,
                                         IQueryable<Registration> registrations,
                                         IQueryable<Event> events,
                                         IRepository<Mail> mails,
                                         IRepository<MailToRegistration> mailsToRegistrations,
                                         MailComposer mailComposer)
    {
        _mailTemplates = mailTemplates;
        _registrations = registrations;
        _events = events;
        _mails = mails;
        _mailsToRegistrations = mailsToRegistrations;
        _mailComposer = mailComposer;
    }

    public async Task<Unit> Handle(CreateBulkMailsCommand command, CancellationToken cancellationToken)
    {
        var templates = await _mailTemplates.Where(mtp => mtp.EventId == command.EventId
                                                       && mtp.BulkMailKey == command.BulkMailKey
                                                       && mtp.Type == 0
                                                       && !mtp.IsDeleted)
                                            .ToListAsync(cancellationToken);

        var registrationsOfEvent = await _registrations.Where(reg => reg.EventId == command.EventId
                                                                  && reg.State != RegistrationState.Cancelled
                                                                  && !reg.Mails.Any(mail =>
                                                                                        mail.Mail.BulkMailKey == command.BulkMailKey))
                                                       .Include(reg => reg.Seats_AsLeader)
                                                       .Include(reg => reg.Seats_AsFollower)
                                                       .ToListAsync(cancellationToken);
        foreach (var mailTemplate in templates)
        {
            var registrationsForTemplate = mailTemplate.RegistrableId == null
                                               ? registrationsOfEvent
                                               : registrationsOfEvent.Where(reg =>
                                                                                reg.Seats_AsLeader.Any(spt =>
                                                                                                           !spt.IsCancelled && spt.RegistrableId == mailTemplate.RegistrableId)
                                                                             || reg.Seats_AsFollower.Any(spt =>
                                                                                                             !spt.IsCancelled && spt.RegistrableId == mailTemplate.RegistrableId))
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
                                                                      && reg.IsWaitingList != true));
            }

            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.WaitingList) == true)
            {
                receivers.AddRange(registrationsForTemplate.Where(reg => reg.State == RegistrationState.Received
                                                                      && (reg.Language == mailTemplate.Language || reg.Language == null)
                                                                      && reg.IsWaitingList == true));
            }

            if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.PredecessorEvent) == true)
            {
                var alreadyCoveredMailAddresses = await _mails.Where(mail => mail.BulkMailKey == command.BulkMailKey)
                                                              .SelectMany(mail => mail.Registrations)
                                                              .Select(map => map.Registration.RespondentEmail)
                                                              .ToListAsync(cancellationToken);

                // distinct by email, not registrationid
                var predecessorReceivers = await _events.Where(evt => evt.Id == command.EventId)
                                                        .SelectMany(evt => evt.PredecessorEvent.Registrations)
                                                        .Where(reg =>
                                                                   !alreadyCoveredMailAddresses.Contains(reg.RespondentEmail))
                                                        .Take(ChunkSize)
                                                        .ToListAsync(cancellationToken);
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.PrePredecessorEvent) == true)
                {
                    predecessorReceivers.AddRange(await _events.Where(evt => evt.Id == command.EventId)
                                                               .SelectMany(evt =>
                                                                               evt.PredecessorEvent.PredecessorEvent.Registrations)
                                                               .Where(reg =>
                                                                          !alreadyCoveredMailAddresses.Contains(
                                                                              reg.RespondentEmail))
                                                               .Take(ChunkSize)
                                                               .ToListAsync(cancellationToken));
                }

                receivers.AddRange(
                    predecessorReceivers.DistinctBy(reg => reg.RespondentEmail.ToLower()).Take(ChunkSize));
            }

            foreach (var registration in receivers.DistinctBy(reg => reg.Id))
            {
                await CreateMail(mailTemplate, registration, cancellationToken);
            }
        }

        return Unit.Value;
    }

    private async Task CreateMail(MailTemplate mailTemplate,
                                  Registration registration,
                                  CancellationToken cancellationToken)
    {
        var content = await _mailComposer.Compose(registration.Id, mailTemplate.Template, mailTemplate.Language,
                                                  cancellationToken);
        var mail = new Mail
                   {
                       Id = Guid.NewGuid(),
                       Created = DateTime.UtcNow,
                       Recipients = registration.RespondentEmail,
                       SenderMail = mailTemplate.SenderMail,
                       SenderName = mailTemplate.SenderName,
                       Subject = mailTemplate.Subject,
                       Withhold = true,
                       BulkMailKey = mailTemplate.BulkMailKey,
                       ContentHtml = content,
                       EventId = mailTemplate.EventId
                       //MailTemplateId = mailTemplate.Id //ToDo
                   };
        await _mails.InsertOrUpdateEntity(mail, cancellationToken);
        await _mailsToRegistrations.InsertOrUpdateEntity(new MailToRegistration
                                                         {
                                                             Id = Guid.NewGuid(),
                                                             RegistrationId = registration.Id,
                                                             MailId = mail.Id
                                                         }, cancellationToken);
    }
}
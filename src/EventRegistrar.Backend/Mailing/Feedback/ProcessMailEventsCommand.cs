using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Feedback;

public class ProcessMailEventsCommand : IRequest
{
    public Guid RawMailEventsId { get; set; }
}

public class ProcessMailEventsCommandHandler(IRepository<RawMailEvent> _rawMailEvents,
                                             IRepository<Mail> mails,
                                             IRepository<MailEvent> mailEvents,
                                             ILogger log,
                                             IDateTimeProvider dateTimeProvider,
                                             ChangeTrigger changeTrigger)
    : IRequestHandler<ProcessMailEventsCommand>
{
    public async Task Handle(ProcessMailEventsCommand command, CancellationToken cancellationToken)
    {
        var rawMailEvents = await _rawMailEvents.FirstAsync(mev => mev.Id == command.RawMailEventsId, cancellationToken);
        if (rawMailEvents.Processed != null)
        {
            log.LogWarning("RawMailEvents with id {0} have already been processed", rawMailEvents.Id);
            return;
        }

        var events = JsonConvert.DeserializeObject<IEnumerable<SendGridEvent>>(rawMailEvents.Body)
                  ?? Enumerable.Empty<SendGridEvent>();
        foreach (var sendGridEvent in events)
        {
            var mail = await GetMail(sendGridEvent);
            if (mail == null)
            {
                continue;
            }

            // dedup
            if (!string.IsNullOrEmpty(sendGridEvent.Sg_event_id))
            {
                var existingEvent = await mailEvents.FirstOrDefaultAsync(mev => mev.ExternalIdentifier == sendGridEvent.Sg_event_id,
                                                                         cancellationToken);
                if (existingEvent != null)
                {
                    log.LogWarning("MailEvent {0} already exists", sendGridEvent.Sg_event_id);
                }
            }

            if (!Enum.TryParse(sendGridEvent.Event, true, out MailState state))
            {
                state = MailState.Unknown;
            }
            else
            {
                mail.State = state;
                // if addressed to multiple emails, save which receiver is concerned
                foreach (var mailToRegistration in mail.Registrations!.Where(mil => mil.Email?.ToLowerInvariant() == sendGridEvent.Email?.ToLowerInvariant()))
                {
                    mailToRegistration.State = state;
                    changeTrigger.TriggerUpdate<RegistrationCalculator>(mailToRegistration.RegistrationId,
                                                                        mailToRegistration.Registration!.EventId);
                }
            }

            var mailEvent = new MailEvent
                            {
                                Id = Guid.NewGuid(),
                                Created = dateTimeProvider.Now,
                                ExternalIdentifier = sendGridEvent.Sg_event_id,
                                MailId = mail.Id,
                                EMail = sendGridEvent.Email,
                                RawEvent = JsonConvert.SerializeObject(sendGridEvent),
                                State = state,
                                Reason = sendGridEvent.Reason,
                                BounceClassification = sendGridEvent.Bounce_Classification
                            };
            mailEvents.InsertObjectTree(mailEvent);
        }

        rawMailEvents.Processed = dateTimeProvider.Now;
    }

    /// <summary>
    /// example of a smtp-id: "<wRf27Si1SgOQhqYl0Iu4_A@ismtpd0002p1lon1.sendgrid.net>"
    /// extract wRf27Si1SgOQhqYl0Iu4_A
    /// </summary>
    private static string? ExtractMessageId(string? smtp_Id)
    {
        if (smtp_Id == null)
        {
            return null;
        }

        var id = smtp_Id?.TrimStart('<');
        id = id?[..id.IndexOf('@')];
        return id;
    }

    private async Task<Mail?> GetMail(SendGridEvent sendGridEvent)
    {
        if (Guid.TryParse(sendGridEvent.MailId, out var mailId))
        {
            var mail = await mails.Where(mil => mil.Id == mailId)
                                  .Include(mil => mil.Registrations!)
                                  .ThenInclude(reg => reg.Registration)
                                  .FirstOrDefaultAsync();
            if (mail != null)
            {
                return mail;
            }
        }

        // fallback to smtp-id
        var messageId = ExtractMessageId(sendGridEvent.Smtp_id);
        return await mails.Where(mil => mil.SendGridMessageId == messageId)
                          .Include(mil => mil.Registrations!)
                          .ThenInclude(reg => reg.Registration)
                          .FirstOrDefaultAsync();
    }
}
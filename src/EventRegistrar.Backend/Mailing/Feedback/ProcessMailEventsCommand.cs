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

        if (rawMailEvents.MailSender == MailSender.SendGrid || rawMailEvents.MailSender == null)
        {
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
                    foreach (var mailToRegistration in mail.Registrations!.Where(mil => string.Equals(mil.Email, sendGridEvent.Email, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        mailToRegistration.State = state;
                        changeTrigger.TriggerUpdate<RegistrationCalculator>(mailToRegistration.RegistrationId,
                                                                            mailToRegistration.Registration!.EventId);
                    }
                }

                var mailEvent = new MailEvent
                                {
                                    Id = Guid.NewGuid(),
                                    MailSender = mail.SentBy,
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
        }
        else if (rawMailEvents.MailSender == MailSender.Postmark)
        {
            if (rawMailEvents.Type == MailState.Delivered)
            {
                var deliveryEvent = JsonConvert.DeserializeObject<PostmarkEventDelivery>(rawMailEvents.Body);
                if (deliveryEvent != null)
                {
                    var mail = await GetMail(deliveryEvent.MessageID);
                    if (mail != null
                     && mail.State != MailState.Open
                     && mail.State != MailState.Click)
                    {
                        var newState = MailState.Delivered;
                        mail.State = newState;
                        // if addressed to multiple emails, save which receiver is concerned
                        foreach (var mailToRegistration in mail.Registrations!.Where(mil => string.Equals(mil.Email, deliveryEvent.Recipient, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            mailToRegistration.State = newState;
                            changeTrigger.TriggerUpdate<RegistrationCalculator>(mailToRegistration.RegistrationId,
                                                                                mailToRegistration.Registration!.EventId);
                        }

                        var mailEvent = new MailEvent
                                        {
                                            Id = Guid.NewGuid(),
                                            MailSender = mail.SentBy,
                                            ExternalIdentifier = deliveryEvent.MessageID,
                                            MailId = mail.Id,
                                            EMail = deliveryEvent.Recipient,
                                            RawEvent = JsonConvert.SerializeObject(deliveryEvent),
                                            State = newState,
                                            Reason = deliveryEvent.Details
                                        };
                        mailEvents.InsertObjectTree(mailEvent);
                    }
                }
            }
            else if (rawMailEvents.Type == MailState.Bounce)
            {
                var bounceEvent = JsonConvert.DeserializeObject<PostmarkEventBounce>(rawMailEvents.Body);
                if (bounceEvent != null)
                {
                    var mail = await GetMail(bounceEvent.MessageID);
                    if (mail != null
                     && mail.State != MailState.Delivered
                     && mail.State != MailState.Open
                     && mail.State != MailState.Click)
                    {
                        var newState = MailState.Bounce;
                        mail.State = newState;
                        // if addressed to multiple emails, save which receiver is concerned
                        foreach (var mailToRegistration in mail.Registrations!.Where(mil => string.Equals(mil.Email, bounceEvent.Email, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            mailToRegistration.State = newState;
                            changeTrigger.TriggerUpdate<RegistrationCalculator>(mailToRegistration.RegistrationId,
                                                                                mailToRegistration.Registration!.EventId);
                        }

                        var mailEvent = new MailEvent
                                        {
                                            Id = Guid.NewGuid(),
                                            MailSender = mail.SentBy,
                                            ExternalIdentifier = bounceEvent.MessageID,
                                            MailId = mail.Id,
                                            EMail = bounceEvent.Email,
                                            RawEvent = JsonConvert.SerializeObject(bounceEvent),
                                            State = newState,
                                            Reason = bounceEvent.Details
                                        };
                        mailEvents.InsertObjectTree(mailEvent);
                    }
                }
            }
            else if (rawMailEvents.Type == MailState.Open)
            {
                var openEvent = JsonConvert.DeserializeObject<PostmarkEventOpen>(rawMailEvents.Body);
                if (openEvent != null)
                {
                    var mail = await GetMail(openEvent.MessageID);
                    if (mail != null
                     && mail.State != MailState.Click)
                    {
                        var newState = MailState.Open;
                        mail.State = newState;
                        // if addressed to multiple emails, save which receiver is concerned
                        foreach (var mailToRegistration in mail.Registrations!.Where(mil => string.Equals(mil.Email, openEvent.Recipient, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            mailToRegistration.State = newState;
                            changeTrigger.TriggerUpdate<RegistrationCalculator>(mailToRegistration.RegistrationId,
                                                                                mailToRegistration.Registration!.EventId);
                        }

                        var mailEvent = new MailEvent
                                        {
                                            Id = Guid.NewGuid(),
                                            MailSender = mail.SentBy,
                                            ExternalIdentifier = openEvent.MessageID,
                                            MailId = mail.Id,
                                            EMail = openEvent.Recipient,
                                            RawEvent = JsonConvert.SerializeObject(openEvent),
                                            State = newState
                                        };
                        mailEvents.InsertObjectTree(mailEvent);
                    }
                }
            }
        }
        else
        {
            throw new NotImplementedException($"MailSender {rawMailEvents.MailSender} not implemented");
        }

        rawMailEvents.Processed = dateTimeProvider.Now;
    }


    private async Task<Mail?> GetMail(SendGridEvent sendGridEvent)
    {
        var mail = await GetMail(sendGridEvent.MailId);
        if (mail != null)
        {
            return mail;
        }

        // fallback to smtp-id
        var messageId = ExtractMessageId(sendGridEvent.Smtp_id);
        return await mails.Where(mil => mil.MailSenderMessageId == messageId)
                          .Include(mil => mil.Registrations!)
                          .ThenInclude(reg => reg.Registration)
                          .FirstOrDefaultAsync();
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

    private async Task<Mail?> GetMail(string? messageId)
    {
        if (messageId != null
         && Guid.TryParse(messageId, out var mailId))
        {
            return await mails.Where(mil => mil.Id == mailId)
                              .Include(mil => mil.Registrations!)
                              .ThenInclude(reg => reg.Registration)
                              .FirstOrDefaultAsync();
        }

        return null;
    }
}
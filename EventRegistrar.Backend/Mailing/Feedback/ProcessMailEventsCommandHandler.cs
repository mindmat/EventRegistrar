using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class ProcessMailEventsCommandHandler : IRequestHandler<ProcessMailEventsCommand>
    {
        private readonly ILogger _log;
        private readonly IRepository<MailEvent> _mailEvents;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<RawMailEvents> _rawMailEvents;

        public ProcessMailEventsCommandHandler(IRepository<RawMailEvents> rawMailEvents,
                                               IRepository<Mail> mails,
                                               IRepository<MailEvent> mailEvents,
                                               ILogger log)
        {
            _rawMailEvents = rawMailEvents;
            _mails = mails;
            _mailEvents = mailEvents;
            _log = log;
        }

        public async Task<Unit> Handle(ProcessMailEventsCommand command, CancellationToken cancellationToken)
        {
            var rawMailEvents = await _rawMailEvents.FirstAsync(mev => mev.Id == command.RawMailEventsId, cancellationToken);
            if (rawMailEvents.Processed)
            {
                return Unit.Value;
            }

            var events = JsonConvert.DeserializeObject<IEnumerable<SendGridEvent>>(rawMailEvents.Body);
            foreach (var sendGridEvent in events)
            {
                var mail = await _mails.FirstOrDefaultAsync(mil => mil.SendGridMessageId == sendGridEvent.Smtp_id, cancellationToken);
                if (mail == null)
                {
                    continue;
                }

                var existingEvent = await _mailEvents.FirstOrDefaultAsync(mev => mev.ExternalIdentifier == sendGridEvent.Sendgrid_event_id, cancellationToken);
                if (existingEvent != null)
                {
                    _log.LogWarning("MailEvent {0} already exists", sendGridEvent.Sendgrid_event_id);
                }

                if (!Enum.TryParse(sendGridEvent.Event, out MailState state))
                {
                    state = MailState.Unknown;
                }
                else
                {
                    mail.State = state;
                }

                var mailEvent = new MailEvent
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.UtcNow,
                    ExternalIdentifier = sendGridEvent.Sendgrid_event_id,
                    MailId = mail.Id,
                    RawEvent = JsonConvert.SerializeObject(sendGridEvent),
                    State = state
                };
                await _mailEvents.InsertOrUpdateEntity(mailEvent, cancellationToken);
            }
            return Unit.Value;
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class ProcessReceivedSmsCommand : IRequest
    {
        public TwilioSms Sms { get; set; }
    }

    public class ProcessReceivedSmsCommandHandler : IRequestHandler<ProcessReceivedSmsCommand>
    {
        private readonly ConfigurationResolver _configurationResolver;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly IEventBus _eventBus;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Sms> _sms;

        public ProcessReceivedSmsCommandHandler(IQueryable<Registration> registrations,
                                                IRepository<Sms> sms,
                                                ConfigurationResolver configurationResolver,
                                                PhoneNormalizer phoneNormalizer,
                                                IEventBus eventBus)
        {
            _registrations = registrations;
            _sms = sms;
            _configurationResolver = configurationResolver;
            _phoneNormalizer = phoneNormalizer;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(ProcessReceivedSmsCommand command, CancellationToken cancellationToken)
        {
            var registrations = await _registrations
                                      .Where(reg => reg.PhoneNormalized == command.Sms.From
                                                 && reg.Event.State != State.Finished)
                                      .Select(reg => new
                                      {
                                          reg.Id,
                                          reg.EventId,
                                          EventState = reg.Event.State,
                                          LastSmsSent = reg.Sms.Select(msg => msg.Sent)
                                                               .OrderByDescending(snt => snt)
                                                               .FirstOrDefault(),
                                          RegistrationState = reg.State,
                                          reg.ReceivedAt,
                                          reg.RespondentFirstName,
                                          reg.RespondentLastName
                                      })
                                      .ToListAsync(cancellationToken);

            // filter to registrations of events that have this number/Twilio account sid configured
            var eventIdsWithThisNumber = registrations.Select(reg => reg.EventId)
                                                      .Distinct()
                                                      .Where(eid => IsSmsAddressedToEvent(eid, command.Sms))
                                                      .ToHashSet();
            registrations = registrations.Where(reg => eventIdsWithThisNumber.Contains(reg.EventId))
                                         .OrderByDescending(reg => reg.EventState == State.RegistrationOpen)
                                         .ThenBy(reg => reg.RegistrationState == RegistrationState.Cancelled)
                                         .ThenByDescending(reg => reg.LastSmsSent ?? DateTime.MinValue)
                                         .ThenByDescending(reg => reg.ReceivedAt)
                                         .ToList();

            var registration = registrations.FirstOrDefault();
            _eventBus.Publish(new SmsReceived
            {
                RegistrationId = registration?.Id,
                Registration = registration == null ? null : $"{registration.RespondentFirstName} {registration.RespondentLastName}",
                From = command.Sms.From,
                Text = command.Sms.Body,
                Received = DateTimeOffset.Now
            });
            if (registration == null)
            {
                return Unit.Value;
            }

            var sms = new Sms
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                SmsSid = command.Sms.SmsSid,
                SmsStatus = command.Sms.SmsStatus,
                Body = command.Sms.Body,
                From = command.Sms.From,
                To = command.Sms.To,
                AccountSid = command.Sms.AccountSid,
                RawData = JsonConvert.SerializeObject(command.Sms),
                Received = DateTime.UtcNow,
            };

            await _sms.InsertOrUpdateEntity(sms, cancellationToken);

            return Unit.Value;
        }

        private bool IsSmsAddressedToEvent(Guid eventId, TwilioSms sms)
        {
            var config = _configurationResolver.GetConfiguration<TwilioConfiguration>(eventId);
            return config.Sid == sms.AccountSid
                && _phoneNormalizer.NormalizePhone(config.Number) == sms.To;
        }
    }
}
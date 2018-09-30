using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class ProcessReceivedSmsCommandHandler : IRequestHandler<ProcessReceivedSmsCommand>
    {
        private readonly ConfigurationResolver _configurationResolver;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Sms> _sms;

        public ProcessReceivedSmsCommandHandler(IQueryable<Registration> registrations,
                                                IRepository<Sms> sms,
                                                ConfigurationResolver configurationResolver,
                                                PhoneNormalizer phoneNormalizer)
        {
            _registrations = registrations;
            _sms = sms;
            _configurationResolver = configurationResolver;
            _phoneNormalizer = phoneNormalizer;
        }

        public async Task<Unit> Handle(ProcessReceivedSmsCommand command, CancellationToken cancellationToken)
        {
            var registrations = await _registrations
                                      .Where(reg => reg.PhoneNormalized == command.Sms.From)
                                      .ToListAsync(cancellationToken);

            // filter to registrations of events that have this number/Twilio account sid configured
            var eventIds = registrations.Select(reg => reg.EventId)
                                        .Distinct()
                                        .Where(eid => IsSmsAddressedToEvent(eid, command.Sms))
                                        .ToHashSet();
            registrations = registrations.Where(reg => eventIds.Contains(reg.EventId))
                                         .OrderBy(reg => reg.State == RegistrationState.Cancelled)
                                         .ThenByDescending(reg => reg.ReceivedAt)
                                         .ToList();

            var registrationId = registrations.FirstOrDefault()?.Id;

            var sms = new Sms
            {
                Id = Guid.NewGuid(),
                RegistrationId = registrationId,
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
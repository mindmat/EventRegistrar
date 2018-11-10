using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SendSmsCommandHandler : IRequestHandler<SendSmsCommand>
    {
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Sms> _sms;
        private readonly TwilioConfiguration _twilioConfiguration;

        public SendSmsCommandHandler(IQueryable<Registration> registrations,
                                     IRepository<Sms> sms,
                                     TwilioConfiguration twilioConfiguration)
        {
            _registrations = registrations;
            _sms = sms;
            _twilioConfiguration = twilioConfiguration;
        }

        public async Task<Unit> Handle(SendSmsCommand command, CancellationToken cancellationToken)
        {
            if (_twilioConfiguration.Sid == null || _twilioConfiguration.Token == null)
            {
                throw new Exception("No Twilio SID/Token found");
            }

            var phone = await _registrations.Where(reg => reg.Id == command.RegistrationId
                                                       && reg.EventId == command.EventId)
                                            .Select(reg => reg.PhoneNormalized)
                                            .FirstOrDefaultAsync(cancellationToken);

            if (phone == null)
            {
                throw new Exception("No number found in registration");
            }

            TwilioClient.Init(_twilioConfiguration.Sid, _twilioConfiguration.Token);

            var callbackUrl = new Uri($"https://eventregistrarfunctions.azurewebsites.net/api/events/{command.EventId}/sms/setStatus");

            var message = await MessageResource.CreateAsync(phone,
                                                            from: _twilioConfiguration.Number,
                                                            body: command.Message,
                                                            statusCallback: callbackUrl);

            var sms = new Sms
            {
                Id = Guid.NewGuid(),
                RegistrationId = command.RegistrationId,
                SmsSid = message.Sid,
                SmsStatus = message.Status.ToString(),
                Body = message.Body,
                From = message.From.ToString(),
                To = message.To,
                AccountSid = message.AccountSid,
                Sent = DateTime.UtcNow,
                Price = $"{message.Price}{message.PriceUnit}",
                ErrorCode = message.ErrorCode,
                Error = message.ErrorMessage,
                Type = SmsType.Reminder,
                RawData = JsonConvert.SerializeObject(message)
            };

            await _sms.InsertOrUpdateEntity(sms, cancellationToken);

            return Unit.Value;
        }
    }
}
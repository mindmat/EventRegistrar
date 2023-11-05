﻿using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrations;

using Newtonsoft.Json;

using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EventRegistrar.Backend.PhoneMessages;

public class SendSmsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string Message { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SendSmsCommandHandler : IRequestHandler<SendSmsCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IRepository<Sms> _sms;
    private readonly TwilioConfiguration _twilioConfiguration;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ChangeTrigger _changeTrigger;

    public SendSmsCommandHandler(IQueryable<Registration> registrations,
                                 IRepository<Sms> sms,
                                 TwilioConfiguration twilioConfiguration,
                                 IEventBus eventBus,
                                 IDateTimeProvider dateTimeProvider,
                                 ChangeTrigger changeTrigger)
    {
        _registrations = registrations;
        _sms = sms;
        _twilioConfiguration = twilioConfiguration;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
        _changeTrigger = changeTrigger;
    }

    public async Task<Unit> Handle(SendSmsCommand command, CancellationToken cancellationToken)
    {
        if (_twilioConfiguration.Sid == null || _twilioConfiguration.Token == null)
        {
            throw new Exception("No Twilio SID/Token found");
        }

        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId
                                                          && reg.EventId == command.EventId)
                                               .FirstOrDefaultAsync(cancellationToken);

        if (registration.PhoneNormalized == null)
        {
            throw new Exception("No number found in registration");
        }

        TwilioClient.Init(_twilioConfiguration.Sid, _twilioConfiguration.Token);

        var callbackUrl = new Uri($"https://event-admin-functions.azurewebsites.net/api/events/{command.EventId}/sms/setStatus");

        var message = await MessageResource.CreateAsync(registration.PhoneNormalized,
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
                      Sent = _dateTimeProvider.Now,
                      Price = $"{message.Price}{message.PriceUnit}",
                      ErrorCode = message.ErrorCode,
                      Error = message.ErrorMessage,
                      Type = SmsType.Reminder,
                      RawData = JsonConvert.SerializeObject(message)
                  };

        _sms.InsertObjectTree(sms);

        _eventBus.Publish(new SmsSent
                          {
                              RegistrationId = registration?.Id,
                              EventId = registration?.EventId,
                              To = registration.PhoneNormalized,
                              Text = command.Message,
                              Sent = _dateTimeProvider.Now
                          });
        _changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);


        return Unit.Value;
    }
}
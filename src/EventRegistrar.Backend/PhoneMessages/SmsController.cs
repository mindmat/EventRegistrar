using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SmsController
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public SmsController(IMediator mediator,
                             IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms")]
        public async Task<IEnumerable<SmsDisplayItem>> GetSmsConversation(string eventAcronym, Guid registrationId)
        {
            return await _mediator.Send(new SmsConversationQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms/send")]
        public async Task SendSms(string eventAcronym, Guid registrationId, [FromBody]SmsContent message)
        {
            await _mediator.Send(new SendSmsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                Message = message?.Body
            });
        }

        public class SmsContent
        {
            public string Body { get; set; }
        }
    }
}
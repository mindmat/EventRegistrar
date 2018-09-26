using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SmsController
    {
        private readonly IMediator _mediator;

        public SmsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms")]
        public Task<IEnumerable<SmsDisplayItem>> GetSmsConversation(string eventAcronym, Guid registrationId)
        {
            return _mediator.Send(new SmsConversationQuery
            {
                EventAcronym = eventAcronym,
                RegistrationId = registrationId,
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms/send")]
        public Task SendSms(string eventAcronym, Guid registrationId, [FromBody]SmsContent message)
        {
            return _mediator.Send(new SendSmsCommand
            {
                EventAcronym = eventAcronym,
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
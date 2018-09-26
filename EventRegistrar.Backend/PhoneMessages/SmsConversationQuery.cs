using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SmsConversationQuery : IRequest<IEnumerable<SmsDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
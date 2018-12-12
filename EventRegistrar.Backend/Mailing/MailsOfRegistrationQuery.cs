using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsOfRegistrationQuery : IRequest<IEnumerable<MailDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
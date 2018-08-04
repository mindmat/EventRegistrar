using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsOfRegistrationQuery : IRequest<IEnumerable<Mail>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
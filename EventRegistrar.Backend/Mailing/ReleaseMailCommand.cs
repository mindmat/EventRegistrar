using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class ReleaseMailCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid MailId { get; set; }
    }
}
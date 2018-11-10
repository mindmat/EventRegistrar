using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class DeleteMailCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid MailId { get; set; }
    }
}
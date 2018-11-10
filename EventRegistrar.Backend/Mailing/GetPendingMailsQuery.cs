using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class GetPendingMailsQuery : IRequest<IEnumerable<Mail>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class GetPendingMailsQuery : IRequest<IEnumerable<Mail>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
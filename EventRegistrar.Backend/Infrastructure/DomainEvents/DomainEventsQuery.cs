using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEventDisplayItem
    {
        public DateTimeOffset? Timestamp { get; set; }
    }

    public class DomainEventsQuery : IRequest<IEnumerable<DomainEventDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class DomainEventsQueryHandler : IRequestHandler<DomainEventsQuery, IEnumerable<DomainEventDisplayItem>>
    {
        private readonly IQueryable<PersistedDomainEvent> _domainEvents;

        public DomainEventsQueryHandler(IQueryable<PersistedDomainEvent> domainEvents)
        {
            _domainEvents = domainEvents;
        }

        public async Task<IEnumerable<DomainEventDisplayItem>> Handle(DomainEventsQuery query, CancellationToken cancellationToken)
        {
            return await _domainEvents.Where(evt => evt.EventId == query.EventId)
                                      .Take(100)
                                      .Select(evt => new DomainEventDisplayItem
                                      {
                                          Timestamp = evt.Timestamp,
                                          //evt.
                                      })
                                      .ToListAsync();
        }
    }
}

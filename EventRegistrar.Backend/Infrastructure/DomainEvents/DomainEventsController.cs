using System.Collections.Generic;
using System.Threading.Tasks;

using EventRegistrar.Backend.Events;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEventsController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public DomainEventsController(IMediator mediator,
                                      IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/domainevents")]
        public async Task<IEnumerable<DomainEventDisplayItem>> GetRecentEvent(string eventAcronym, IEnumerable<string> types)
        {
            return await _mediator.Send(new DomainEventsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                Types = types
            });
        }

        [HttpGet("api/events/{eventAcronym}/domaineventtypes")]
        public Task<IEnumerable<DomainEventCatalogItem>> GetDomainEventCatalog(DomainEventCatalogQuery request)
        {
            return _mediator.Send(new DomainEventCatalogQuery());
        }
    }
}
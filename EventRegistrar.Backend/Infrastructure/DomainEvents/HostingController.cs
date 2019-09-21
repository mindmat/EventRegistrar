using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Hosting
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
        public async Task<HostingOffers> GetRecentEvent(string eventAcronym)
        {
            return await _mediator.Send(new HostingOffersQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }
    }
}
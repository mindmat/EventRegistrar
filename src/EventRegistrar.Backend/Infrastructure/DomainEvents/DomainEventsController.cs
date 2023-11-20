using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class DomainEventsController(IMediator mediator,
                                    IEventAcronymResolver eventAcronymResolver)
    : Controller
{
    [HttpGet("api/events/{eventAcronym}/domainevents")]
    public async Task<IEnumerable<DomainEventDisplayItem>> GetRecentEvent(
        string eventAcronym,
        IEnumerable<string> types)
    {
        return await mediator.Send(new DomainEventsQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                       Types = types
                                   });
    }

    [HttpGet("api/events/{eventAcronym}/domaineventtypes")]
    public Task<IEnumerable<DomainEventCatalogItem>> GetDomainEventCatalog(DomainEventCatalogQuery request)
    {
        return mediator.Send(new DomainEventCatalogQuery());
    }
}
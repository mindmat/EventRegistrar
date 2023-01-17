using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Unassigned;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments;

public class PaymentController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator,
                             IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpGet("api/events/{eventAcronym}/payouts/unassigned")]
    public async Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPayouts(string eventAcronym)
    {
        return await _mediator.Send(new UnassignedPayoutsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }
}
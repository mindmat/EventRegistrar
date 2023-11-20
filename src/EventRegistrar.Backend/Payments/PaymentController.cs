using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Unassigned;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments;

public class PaymentController(IMediator mediator,
                               IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpGet("api/events/{eventAcronym}/payouts/unassigned")]
    public async Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPayouts(string eventAcronym)
    {
        return await mediator.Send(new UnassignedPayoutsQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                   });
    }
}
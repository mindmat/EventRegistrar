using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Differences;

public class DifferencesController(IMediator mediator,
                                   IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpGet("api/events/{eventAcronym}/differences")]
    public async Task<IEnumerable<DifferencesDisplayItem>> GetRefunds(string eventAcronym)
    {
        return await mediator.Send(new DifferencesQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                   });
    }

    [HttpPost("api/events/{eventAcronym}/registration/{registrationId:guid}/sendPaymentDueMail")]
    public async Task SendPaymentDueMail(string eventAcronym, Guid registrationId)
    {
        await mediator.Send(new SendPaymentDueMailCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationId = registrationId
                            });
    }

    [HttpPost("api/events/{eventAcronym}/registration/{registrationId:guid}/refundDifference")]
    public async Task RefundDifference(string eventAcronym, Guid registrationId)
    {
        await mediator.Send(new RefundDifferenceCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationId = registrationId
                            });
    }
}
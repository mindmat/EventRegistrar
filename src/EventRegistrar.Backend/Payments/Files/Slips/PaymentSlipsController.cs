using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipsController(IMediator mediator,
                                    IEventAcronymResolver eventAcronymResolver)
    : Controller
{
    [HttpGet("api/events/{eventAcronym}/paymentslips/{paymentSlipId:guid}")]
    public async Task<FileContentResult> GetPaymentSlipImage(string eventAcronym, Guid paymentSlipId)
    {
        return await mediator.Send(new PaymentSlipImageQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                       PaymentSlipId = paymentSlipId
                                   });
    }
}
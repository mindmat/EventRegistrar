using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipsController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PaymentSlipsController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpGet("api/events/{eventAcronym}/paymentslips/{paymentSlipId:guid}")]
    public async Task<FileContentResult> GetPaymentSlipImage(string eventAcronym, Guid paymentSlipId)
    {
        return await _mediator.Send(new PaymentSlipImageQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        PaymentSlipId = paymentSlipId
                                    });
    }
}
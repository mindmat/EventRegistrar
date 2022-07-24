using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Refunds;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Assignments;

[Route("api/events/{eventAcronym}")]
public class PaymentAssignmentController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PaymentAssignmentController(IMediator mediator,
                                       IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }


    //[HttpPost("payments/{paymentId:guid}/assignToRepayment/{paymentIdOutgoing:guid}")]
    //public async Task AssignRepayment(string eventAcronym,
    //                                  Guid paymentId,
    //                                  Guid paymentIdOutgoing,
    //                                  decimal amount)
    //{
    //    await _mediator.Send(new AssignOutgoingPaymentCommand
    //                         {
    //                             EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
    //                             IncomingPaymentId = paymentId,
    //                             PaymentId_Outgoing = paymentIdOutgoing,
    //                             Amount = amount
    //                         });
    //}

    [HttpGet("payments/{paymentId:guid}/possibleOutgoingAssignments")]
    public async Task<IEnumerable<PossibleRepaymentAssignment>> GetPossibleOutgoingPaymentsAssignments(
        string eventAcronym,
        Guid paymentId)
    {
        return await _mediator.Send(new PossibleRepaymentAssignmentQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        PaymentId = paymentId
                                    });
    }
}
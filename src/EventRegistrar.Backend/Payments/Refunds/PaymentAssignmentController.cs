using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Refunds;

public class PayoutAssignmentController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PayoutAssignmentController(IMediator mediator,
                                      IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpPost("api/events/{eventAcronym}/payouts/{paymentId:guid}/assign/{payoutRequestId:guid}")]
    public async Task AssignPayment(string eventAcronym, Guid paymentId, Guid payoutRequestId, decimal amount,
                                    bool acceptDifference, string acceptDifferenceReason)
    {
        await _mediator.Send(new AssignPayoutCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 PaymentId = paymentId,
                                 PayoutRequestId = payoutRequestId,
                                 Amount = amount,
                                 AcceptDifference = acceptDifference,
                                 AcceptDifferenceReason = acceptDifferenceReason
                             });
    }

    //[HttpPost("api/events/{eventAcronym}/payouts/{paymentId:guid}/assignToRepayment/{paymentIdOutgoing:guid}")]
    //public async Task AssignRepayment(string eventAcronym, Guid paymentId, Guid paymentIdOutgoing, decimal amount)
    //{
    //    await _mediator.Send(new AssignRepaymentCommand
    //    {
    //        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
    //        PaymentId_Incoming = paymentId,
    //        PaymentId_Outgoing = paymentIdOutgoing,
    //        Amount = amount
    //    });
    //}

    //[HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/assignedPayments")]
    //public async Task<IEnumerable<AssignedPaymentDisplayItem>> GetAssignedPaymentsOfRegistration(string eventAcronym, Guid registrationId)
    //{
    //    return await _mediator.Send(new AssignedPaymentsOfRegistrationQuery
    //    {
    //        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
    //        RegistrationId = registrationId
    //    });
    //}

    [HttpGet("api/events/{eventAcronym}/payouts/{paymentId:guid}/possibleAssignments")]
    public async Task<IEnumerable<PossiblePayoutAssignment>> GetPossibleAssignments(string eventAcronym, Guid paymentId)
    {
        return await _mediator.Send(new PossiblePayoutAssignmentQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        PaymentId = paymentId
                                    });
    }

    //[HttpDelete("api/events/{eventAcronym}/paymentAssignments/{paymentAssignmentId:guid}")]
    //public async Task UnassignPayment(string eventAcronym, Guid paymentAssignmentId)
    //{
    //    await _mediator.Send(new UnassignPaymentCommand
    //    {
    //        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
    //        PaymentAssignmentId = paymentAssignmentId
    //    });
    //}
}
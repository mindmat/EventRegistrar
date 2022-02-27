using EventRegistrar.Backend.Events;

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

    [HttpPost("payments/{paymentId:guid}/assign/{registrationId:guid}")]
    public async Task AssignPayment(string eventAcronym, Guid paymentId, Guid registrationId, decimal amount,
                                    bool acceptDifference, string acceptDifferenceReason)
    {
        await _mediator.Send(new AssignPaymentCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 PaymentId = paymentId,
                                 RegistrationId = registrationId,
                                 Amount = amount,
                                 AcceptDifference = acceptDifference,
                                 AcceptDifferenceReason = acceptDifferenceReason
                             });
    }

    [HttpPost("payments/{paymentId:guid}/assignToRepayment/{paymentIdOutgoing:guid}")]
    public async Task AssignRepayment(string eventAcronym, Guid paymentId, Guid paymentIdOutgoing, decimal amount)
    {
        await _mediator.Send(new AssignRepaymentCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 PaymentId_Incoming = paymentId,
                                 PaymentId_Outgoing = paymentIdOutgoing,
                                 Amount = amount
                             });
    }

    [HttpGet("registrations/{registrationId:guid}/assignedPayments")]
    public async Task<IEnumerable<AssignedPaymentDisplayItem>> GetAssignedPaymentsOfRegistration(
        string eventAcronym, Guid registrationId)
    {
        return await _mediator.Send(new AssignedPaymentsOfRegistrationQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        RegistrationId = registrationId
                                    });
    }

    [HttpGet("accounting/bankAccountBookingId/{bankAccountBookingId:guid}/assignmentCandidates")]
    public async Task<IEnumerable<PossibleAssignment>> GetPossibleAssignments(string eventAcronym, Guid bankAccountBookingId)
    {
        return await _mediator.Send(new PossibleAssignmentsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        BankAccountBookingId = bankAccountBookingId
                                    });
    }

    [HttpGet("payments/{paymentId:guid}/possibleOutgoingAssignments")]
    public async Task<IEnumerable<PossibleRepaymentAssignment>> GetPossibleOutgoingPaymentsAssignments(
        string eventAcronym, Guid paymentId)
    {
        return await _mediator.Send(new PossibleRepaymentAssignmentQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        PaymentId = paymentId
                                    });
    }

    [HttpDelete("paymentAssignments/{paymentAssignmentId:guid}")]
    public async Task UnassignPayment(string eventAcronym, Guid paymentAssignmentId)
    {
        await _mediator.Send(new UnassignPaymentCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 PaymentAssignmentId = paymentAssignmentId
                             });
    }
}
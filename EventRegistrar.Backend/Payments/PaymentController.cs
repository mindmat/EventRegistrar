using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Payments.Unassigned;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments
{
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

        [HttpPost("api/events/{eventAcronym}/payments/{paymentId:guid}/assign/{registrationId:guid}")]
        public async Task AssignPayment(string eventAcronym, Guid paymentId, Guid registrationId, decimal amount, bool acceptDifference, string acceptDifferenceReason)
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

        [HttpGet("api/events/{eventAcronym}/duepayments")]
        public async Task<IEnumerable<DuePaymentItem>> GetDuePayments(string eventAcronym)
        {
            return await _mediator.Send(new DuePaymentsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpGet("api/events/{eventAcronym}/payments/overview")]
        public async Task<PaymentOverview> GetPaymentOverview(string eventAcronym)
        {
            return await _mediator.Send(new PaymentOverviewQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpGet("api/events/{eventAcronym}/payments")]
        public async Task<IEnumerable<PaymentDisplayItem>> GetPayments(string eventAcronym)
        {
            return await _mediator.Send(new PaymentStatementsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpGet("api/events/{eventAcronym}/payments/{paymentId:guid}/possibleAssignments")]
        public async Task<IEnumerable<PossibleAssignment>> GetPossibleAssignments(string eventAcronym, Guid paymentId)
        {
            return await _mediator.Send(new PossibleAssignmentsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                PaymentId = paymentId
            });
        }

        [HttpGet("api/events/{eventAcronym}/payments/unassigned")]
        public async Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPaymentsPaymentOverview(string eventAcronym)
        {
            return await _mediator.Send(new UnassignedPaymentsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/sendReminder")]
        public async Task SendReminder(string eventAcronym, Guid registrationId, bool withholdMail)
        {
            await _mediator.Send(new SendReminderCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }
    }
}
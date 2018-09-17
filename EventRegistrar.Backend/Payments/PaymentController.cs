using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/events/{eventAcronym}/payments/{paymentId:guid}/assign/{registrationId:guid}")]
        public Task AssignPayment(string eventAcronym, Guid paymentId, Guid registrationId, decimal amount, bool acceptDifference, string acceptDifferenceReason)
        {
            return _mediator.Send(new AssignPaymentCommand
            {
                EventAcronym = eventAcronym,
                PaymentId = paymentId,
                RegistrationId = registrationId,
                Amount = amount,
                AcceptDifference = acceptDifference,
                AcceptDifferenceReason = acceptDifferenceReason
            });
        }

        [HttpGet("api/events/{eventAcronym}/duepayments")]
        public Task<IEnumerable<DuePaymentItem>> GetDuePayments(string eventAcronym)
        {
            return _mediator.Send(new DuePaymentsQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments/overview")]
        public Task<PaymentOverview> GetPaymentOverview(string eventAcronym)
        {
            return _mediator.Send(new PaymentOverviewQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments")]
        public Task<IEnumerable<PaymentDisplayItem>> GetPayments(string eventAcronym)
        {
            return _mediator.Send(new PaymentStatementsQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments/{paymentId:guid}/possibleAssignments")]
        public Task<IEnumerable<PossibleAssignment>> GetPossibleAssignments(string eventAcronym, Guid paymentId)
        {
            return _mediator.Send(new PossibleAssignmentsQuery { EventAcronym = eventAcronym, PaymentId = paymentId });
        }

        [HttpGet("api/events/{eventAcronym}/payments/unassigned")]
        public Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPaymentsPaymentOverview(string eventAcronym)
        {
            return _mediator.Send(new UnassignedPaymentsQuery { EventAcronym = eventAcronym });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/sendReminder")]
        public Task SendReminder(string eventAcronym, Guid registrationId, bool withholdMail)
        {
            return _mediator.Send(new SendReminderCommand { EventAcronym = eventAcronym, RegistrationId = registrationId });
        }
    }
}
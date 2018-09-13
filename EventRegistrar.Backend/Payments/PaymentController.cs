using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Payments.Unrecognized;
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
        public Task<IEnumerable<PaymentDisplayItem>> GetPayments(string eventAcronym, bool unrecognized)
        {
            return _mediator.Send(new PaymentStatementsQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments/{paymentId:guid}/possibleAssignments")]
        public Task<IEnumerable<PossibleAssignment>> GetPossibleAssignments(string eventAcronym, Guid paymentId)
        {
            return _mediator.Send(new PossibleAssignmentsQuery { EventAcronym = eventAcronym, PaymentId = paymentId });
        }

        [HttpGet("api/events/{eventAcronym}/payments/unrecognized")]
        public Task<IEnumerable<PaymentDisplayItem>> GetUnrecognizedPaymentsPaymentOverview(string eventAcronym)
        {
            return _mediator.Send(new UnrecognizedPaymentsQuery { EventAcronym = eventAcronym });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/sendReminder")]
        public Task SendReminder(string eventAcronym, Guid registrationId, bool withholdMail)
        {
            return _mediator.Send(new SendReminderCommand { EventAcronym = eventAcronym, RegistrationId = registrationId });
        }

        //[HttpPost("api/events/{eventAcronym}/registrations/${registrationId:guid}/sendReminderSms")]
        //public Task SendReminderSms(string eventAcronym, Guid registrationId)
        //{
        //    //return _mediator.Send(new DuePaymentsQuery { EventAcronym = eventAcronym });
        //}

        [HttpPost("api/events/{eventAcronym}/payments/{paymentId:guid}/RecognizedEmail")]
        public Task SetRecognizedEmail(string eventAcronym, Guid paymentId, string email)
        {
            return _mediator.Send(new SetRecognizedEmailCommand { EventAcronym = eventAcronym, PaymentId = paymentId, Email = email });
        }
    }
}
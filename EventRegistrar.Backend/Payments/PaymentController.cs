using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Payments.Due;
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
        public Task GetDuePayments(string eventAcronym)
        {
            return _mediator.Send(new DuePaymentsQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments/overview")]
        public Task<PaymentOverview> GetPaymentOverview(string eventAcronym)
        {
            return _mediator.Send(new PaymentOverviewQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/payments")]
        public Task<IEnumerable<UnrecognizedPaymentDisplayItem>> GetUnregocinzedPaymentsPaymentOverview(string eventAcronym, bool unrecognized)
        {
            return _mediator.Send(new UnrecognizedPaymentsQuery { EventAcronym = eventAcronym });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/${registrationId:guid}/sendReminder")]
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
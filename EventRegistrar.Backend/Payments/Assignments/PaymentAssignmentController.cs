using System;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Assignments
{
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

        [HttpDelete("api/events/{eventAcronym}/paymentAssignments/{paymentAssignmentId:guid}")]
        public async Task UnassignPayment(string eventAcronym, Guid paymentAssignmentId)
        {
            await _mediator.Send(new UnassignPaymentCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                PaymentAssignmentId = paymentAssignmentId
            });
        }
    }
}
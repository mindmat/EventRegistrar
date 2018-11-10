using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignPaymentCommand : IRequest, IEventBoundRequest
    {
        public bool AcceptDifference { get; set; }
        public string AcceptDifferenceReason { get; set; }
        public decimal Amount { get; set; }
        public Guid EventId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
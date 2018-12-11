using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignRepaymentCommand : IRequest, IEventBoundRequest
    {
        public decimal Amount { get; set; }
        public Guid EventId { get; set; }
        public Guid PaymentId_Incoming { get; set; }
        public Guid PaymentId_Outgoing { get; set; }
    }
}
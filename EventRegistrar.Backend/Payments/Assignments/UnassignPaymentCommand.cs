using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class UnassignPaymentCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid PaymentAssignmentId { get; set; }
    }
}
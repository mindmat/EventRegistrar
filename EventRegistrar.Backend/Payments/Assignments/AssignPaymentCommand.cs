using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignPaymentCommand : IRequest, IEventBoundRequest
    {
        public decimal Amount { get; set; }
        public string EventAcronym { get; set; }
        public Guid PaymentId { get; set; }
        public Guid RegistratrionId { get; set; }
    }
}
using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Unrecognized
{
    public class SetRecognizedEmailCommand : IRequest, IEventBoundRequest
    {
        public string Email { get; set; }
        public string EventAcronym { get; set; }
        public Guid PaymentId { get; set; }
    }
}
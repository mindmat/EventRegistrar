using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignedPaymentsOfRegistrationQuery : IRequest<IEnumerable<AssignedPaymentDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
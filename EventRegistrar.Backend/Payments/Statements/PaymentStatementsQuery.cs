using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Unassigned;
using MediatR;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class PaymentStatementsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
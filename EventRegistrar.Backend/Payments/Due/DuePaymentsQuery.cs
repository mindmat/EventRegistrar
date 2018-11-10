using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Due
{
    public class DuePaymentsQuery : IRequest<IEnumerable<DuePaymentItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
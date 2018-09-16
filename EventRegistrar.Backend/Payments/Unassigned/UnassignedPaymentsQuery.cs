using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Unassigned
{
    public class UnassignedPaymentsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
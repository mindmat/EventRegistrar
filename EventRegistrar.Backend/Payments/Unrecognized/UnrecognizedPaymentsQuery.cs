using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Unrecognized
{
    public class UnrecognizedPaymentsQuery : IRequest<IEnumerable<UnrecognizedPaymentDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Unrecognized;
using MediatR;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class PaymentStatementsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
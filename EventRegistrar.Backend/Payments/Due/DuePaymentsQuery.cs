using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Due
{
    public class DuePaymentsQuery : IRequest<IEnumerable<DuePaymentItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentOverviewQuery : IRequest<PaymentOverview>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
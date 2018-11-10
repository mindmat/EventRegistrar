using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentOverviewQuery : IRequest<PaymentOverview>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
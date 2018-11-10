using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class CancelRegistrationCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public bool IgnorePayments { get; set; }
        public string Reason { get; set; }
        public decimal RefundPercentage { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
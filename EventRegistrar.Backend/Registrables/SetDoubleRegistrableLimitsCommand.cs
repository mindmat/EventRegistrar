using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class SetDoubleRegistrableLimitsCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public int MaximumCouples { get; set; }
        public int MaximumImbalance { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
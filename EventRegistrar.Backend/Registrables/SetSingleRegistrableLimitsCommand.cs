using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class SetSingleRegistrableLimitsCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public int MaximumParticipants { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
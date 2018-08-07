using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables.Participants
{
    public class ParticipantsOfRegistrableQuery : IRequest<RegistrableDisplayInfo>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
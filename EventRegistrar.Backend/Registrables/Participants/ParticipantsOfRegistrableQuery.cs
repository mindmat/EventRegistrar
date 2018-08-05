using System;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrables.Participants;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class ParticipantsOfRegistrableQuery : IRequest<RegistrableDisplayInfo>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
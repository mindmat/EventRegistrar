using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Spots
{
    public class AddSpotCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
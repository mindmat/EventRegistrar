using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations
{
    public class RegistrationQuery : IRequest<RegistrationDisplayItem>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
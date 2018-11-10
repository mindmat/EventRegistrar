using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class SaveRegistrationFormDefinitionCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public string FormId { get; set; }
    }
}
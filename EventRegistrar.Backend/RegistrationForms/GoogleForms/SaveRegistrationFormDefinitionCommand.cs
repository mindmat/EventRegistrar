﻿using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class SaveRegistrationFormDefinitionCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public string FormId { get; set; }
    }
}
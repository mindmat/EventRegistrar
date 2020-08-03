using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.Registrations.Register;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationFormController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public RegistrationFormController(IMediator mediator,
                                          IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPost("api/events/{eventAcronym}/registrationForms/{formId}")]
        public async Task SaveRegistrationFormDefinition(string eventAcronym, string formId)
        {
            await _mediator.Send(new SaveRegistrationFormDefinitionCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                FormId = formId
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrationForms/pending")]
        public async Task<IEnumerable<RegistrationFormItem>> GetPendingRegistrationForms(string eventAcronym)
        {
            return await _mediator.Send(new PendingRegistrationFormQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpGet("api/registrationFormTypes")]
        public async Task<IEnumerable<RegistrationFormType>> GetRegistrationFormTypes()
        {
            return await _mediator.Send(new RegistrationFormTypesQuery());
        }

        [HttpGet("api/events/{eventAcronym}/registrationForms")]
        public async Task<IEnumerable<RegistrationFormMappings>> GetRegistrationForms(string eventAcronym)
        {
            return await _mediator.Send(new RegistrationFormsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrationForms/{formId}/mappings")]
        public async Task SaveRegistrationFormMappings(string eventAcronym, Guid formId, [FromBody] RegistrationFormMappings mappings)
        {
            await _mediator.Send(new SaveRegistrationFormMappingsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                Mappings = mappings
            });
        }

        [HttpDelete("api/events/{eventAcronym}/registrationForms/{registrationFormId}")]
        public async Task DeleteRegistrationForm(string eventAcronym, Guid registrationFormId)
        {
            await _mediator.Send(new DeleteRegistrationFormCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationFormId = registrationFormId
            });
        }


        [HttpGet("api/events/{eventAcronym}/formPaths")]
        public async Task<IEnumerable<IRegistrationProcessConfiguration>> GetFormPaths(string eventAcronym)
        {
            return await _mediator.Send(new FormPathsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrationForms/{registrationFormId}/formPaths/{formPathId}/mappings")]
        public async Task SaveRegistrationFormMappings(string eventAcronym,
                                                       Guid registrationFormId,
                                                       Guid formPathId,
                                                       [FromBody] IRegistrationProcessConfiguration configuration)
        {
            await _mediator.Send(new SaveFormPathsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationFormId = registrationFormId,
                Configuration = configuration
            });
        }
    }
}
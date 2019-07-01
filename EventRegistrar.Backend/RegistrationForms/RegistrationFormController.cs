using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
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

        [HttpPost("api/events/{eventAcronym}/registrationforms/{formId}")]
        public async Task SaveRegistrationFormDefinition(string eventAcronym, string formId)
        {
            await _mediator.Send(new SaveRegistrationFormDefinitionCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                FormId = formId
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrationforms/pending")]
        public async Task<IEnumerable<RegistrationFormItem>> GetPendingRegistrationForms(string eventAcronym)
        {
            return await _mediator.Send(new PendingRegistrationFormQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }
    }
}
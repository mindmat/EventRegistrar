using System.Threading.Tasks;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationFormController : Controller
    {
        private readonly IMediator _mediator;

        public RegistrationFormController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/registrationforms/{formId}")]
        public Task SaveRegistrationFormDefinition(string eventAcronym, string formId, FormDescription formDescription)
        {
            return _mediator.Send(new SaveRegistrationFormDefinitionCommand { EventAcronym = eventAcronym, FormId = formId, Description = formDescription });
        }
    }
}
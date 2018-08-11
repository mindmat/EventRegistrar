using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionController : Controller
    {
        private readonly IMediator _mediator;

        public QuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/questions/mapping")]
        public Task<IEnumerable<QuestionToRegistrablesDisplayItem>> GetMapping(string eventAcronym, string formId)
        {
            return _mediator.Send(new QuestionToRegistrablesQuery { EventAcronym = eventAcronym });
        }
    }
}
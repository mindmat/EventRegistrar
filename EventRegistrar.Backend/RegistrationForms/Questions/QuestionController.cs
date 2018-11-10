using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public QuestionController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/questions/mapping")]
        public async Task<IEnumerable<QuestionToRegistrablesDisplayItem>> GetMapping(string eventAcronym, string formId)
        {
            return await _mediator.Send(new QuestionToRegistrablesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
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

        [HttpGet("api/events/{eventAcronym}/questions/unassignedOptions")]
        public async Task<IEnumerable<QuestionToRegistrablesDisplayItem>> GetUnassignedOptions(string eventAcronym, string formId)
        {
            return await _mediator.Send(new UnassignedQuestionOptionsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpPut("api/events/{eventAcronym}/questionoptions/{questionOptionId:guid}/registrables/{registrableId:guid}")]
        public async Task AssignQuestionOptionToRegistrable(string eventAcronym, Guid questionOptionId, Guid registrableId)
        {
            await _mediator.Send(new AssignQuestionOptionToRegistrableCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrableId = registrableId,
                QuestionOptionId = questionOptionId
            });
        }

        [HttpDelete("api/events/{eventAcronym}/questionoptions/{questionOptionId:guid}/registrables/{registrableId:guid}")]
        public async Task RemoveQuestionOptionFromRegistrable(string eventAcronym, Guid questionOptionId, Guid registrableId)
        {
            await _mediator.Send(new RemoveQuestionOptionFromRegistrableCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrableId = registrableId,
                QuestionOptionId = questionOptionId
            });
        }
    }
}
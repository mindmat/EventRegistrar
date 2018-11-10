﻿using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplatesController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public MailTemplatesController(IMediator mediator,
                                       IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/mails/languages")]
        public async Task<IEnumerable<LanguageItem>> GetLanguages(string eventAcronym)
        {
            return await _mediator.Send(new LanguagesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpGet("api/events/{eventAcronym}/mailTemplates")]
        public async Task<IEnumerable<MailTemplateItem>> GetMailTemplatesOfRegistration(string eventAcronym)
        {
            return await _mediator.Send(new MailTemplatesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                TemplateGroup = TemplateGroup.AutoGenerated
            });
        }

        [HttpGet("api/events/{eventAcronym}/mails/types")]
        public async Task<IEnumerable<MailTypeItem>> GetMailTypes(string eventAcronym)
        {
            return await _mediator.Send(new MailTypesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpPost("api/events/{eventAcronym}/mailTemplates")]
        public async Task SaveMailTemplate(string eventAcronym, [FromBody]MailTemplateItem template)
        {
            await _mediator.Send(new SaveMailTemplateCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                Template = template
            });
        }
    }
}
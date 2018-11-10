using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Templates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class BulkMailController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public BulkMailController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{templateKey}/createMails")]
        public async Task CreateMails(string eventAcronym, string templateKey)
        {
            await _mediator.Send(new CreateBulkMailsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                BulkMailKey = templateKey
            });
        }

        [HttpGet("api/events/{eventAcronym}/bulkMailTemplates")]
        public async Task<IEnumerable<MailTemplateItem>> GetMailingTemplatesOfRegistration(string eventAcronym)
        {
            return await _mediator.Send(new MailTemplatesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                TemplateGroup = TemplateGroup.BulkMail
            });
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{templateKey}/releaseMails")]
        public async Task ReleaseMails(string eventAcronym, string templateKey)
        {
            await _mediator.Send(new ReleaseBulkMailsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                BulkMailKey = templateKey
            });
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{mailTemplateId:guid}")]
        public async Task SaveMailingTemplate(string eventAcronym, [FromBody]MailTemplateItem template, Guid mailTemplateId)
        {
            await _mediator.Send(new SaveMailTemplateCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                Template = template,
                TemplateId = mailTemplateId
            });
        }
    }
}
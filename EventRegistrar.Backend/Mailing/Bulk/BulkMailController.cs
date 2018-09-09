using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Mailing.Templates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class BulkMailController : Controller
    {
        private readonly IMediator _mediator;

        public BulkMailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{templateKey}/createMails")]
        public Task CreateMails(string eventAcronym, string templateKey)
        {
            return _mediator.Send(new CreateBulkMailsCommand { EventAcronym = eventAcronym, BulkMailKey = templateKey });
        }

        [HttpGet("api/events/{eventAcronym}/bulkMailTemplates")]
        public Task<IEnumerable<MailTemplateItem>> GetMailingTemplatesOfRegistration(string eventAcronym)
        {
            return _mediator.Send(new MailTemplatesQuery { EventAcronym = eventAcronym, TemplateGroup = TemplateGroup.BulkMail });
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{templateKey}/releaseMails")]
        public Task ReleaseMails(string eventAcronym, string templateKey)
        {
            return _mediator.Send(new ReleaseBulkMailsCommand { EventAcronym = eventAcronym, BulkMailKey = templateKey });
        }

        [HttpPost("api/events/{eventAcronym}/bulkMailTemplates/{mailTemplateId:guid}")]
        public Task SaveMailingTemplate(string eventAcronym, [FromBody]MailTemplateItem template, Guid mailTemplateId)
        {
            return _mediator.Send(new SaveMailTemplateCommand { EventAcronym = eventAcronym, Template = template, TemplateId = mailTemplateId });
        }
    }
}
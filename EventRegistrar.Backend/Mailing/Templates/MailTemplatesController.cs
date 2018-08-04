using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplatesController : Controller
    {
        private readonly IMediator _mediator;

        public MailTemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/mailTemplates")]
        public Task<IEnumerable<MailTemplateItem>> GetMailsOfRegistration(string eventAcronym)
        {
            return _mediator.Send(new MailTemplatesQuery { EventAcronym = eventAcronym });
        }
    }
}
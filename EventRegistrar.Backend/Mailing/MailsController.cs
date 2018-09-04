using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsController : Controller
    {
        private readonly IMediator _mediator;

        public MailsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpDelete("api/events/{eventAcronym}/mails/{mailId:guid}")]
        public Task DeleteMail(string eventAcronym, Guid mailId)
        {
            return _mediator.Send(new DeleteMailCommand { EventAcronym = eventAcronym, MailId = mailId });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/mails")]
        public Task<IEnumerable<Mail>> GetMailsOfRegistration(string eventAcronym, Guid registrationId)
        {
            return _mediator.Send(new MailsOfRegistrationQuery { EventAcronym = eventAcronym, RegistrationId = registrationId });
        }

        [HttpGet("api/events/{eventAcronym}/mails/pending")]
        public Task<IEnumerable<Mail>> GetPendingMails(string eventAcronym)
        {
            return _mediator.Send(new GetPendingMailsQuery { EventAcronym = eventAcronym });
        }

        [HttpPost("api/events/{eventAcronym}/mails/{mailId:guid}/release")]
        public Task ReleaseMail(string eventAcronym, Guid mailId)
        {
            return _mediator.Send(new ReleaseMailCommand { EventAcronym = eventAcronym, MailId = mailId });
        }
    }
}
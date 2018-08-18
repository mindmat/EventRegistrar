using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing
{
    public class SpotsController : Controller
    {
        private readonly IMediator _mediator;

        public SpotsController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
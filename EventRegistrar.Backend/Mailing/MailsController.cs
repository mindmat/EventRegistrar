using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Compose;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public MailsController(IMediator mediator,
                               IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/mails/create")]
        public async Task CreateMailForRegistration(string eventAcronym, Guid registrationId, MailType mailType, bool allowDuplicate)
        {
            await _mediator.Send(new ComposeAndSendMailCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                MailType = mailType,
                Withhold = true,
                AllowDuplicate = allowDuplicate
            });
        }

        [HttpDelete("api/events/{eventAcronym}/mails/{mailId:guid}")]
        public async Task DeleteMail(string eventAcronym, Guid mailId)
        {
            await _mediator.Send(new DeleteMailCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                MailId = mailId
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/mails")]
        public async Task<IEnumerable<MailDisplayItem>> GetMailsOfRegistration(string eventAcronym, Guid registrationId)
        {
            return await _mediator.Send(new MailsOfRegistrationQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }

        [HttpGet("api/events/{eventAcronym}/mails/pending")]
        public async Task<IEnumerable<Mail>> GetPendingMails(string eventAcronym)
        {
            return await _mediator.Send(new GetPendingMailsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpPost("api/events/{eventAcronym}/mails/{mailId:guid}/release")]
        public async Task ReleaseMail(string eventAcronym, Guid mailId)
        {
            await _mediator.Send(new ReleaseMailCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                MailId = mailId
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations
{
    public class RegistrationController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public RegistrationController(IMediator mediator,
                                      IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpDelete("api/events/{eventAcronym}/registrations/{registrationId:guid}")]
        public async Task CancelRegistration(string eventAcronym, Guid registrationId, string reason, bool ignorePayments, decimal refundPercentage)
        {
            await _mediator.Send(new CancelRegistrationCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                Reason = reason,
                IgnorePayments = ignorePayments,
                RefundPercentage = refundPercentage
            });
        }

        [HttpGet("api/registrationforms/{formExternalIdentifier}/RegistrationExternalIdentifiers")]
        public Task<IEnumerable<string>> GetAllExternalRegistrationIdentifiers(string formExternalIdentifier)
        {
            return _mediator.Send(new AllExternalRegistrationIdentifiersQuery { RegistrationFormExternalIdentifier = formExternalIdentifier });
        }

        [HttpGet("api/events/{eventAcronym}/registrations")]
        public async Task<IEnumerable<RegistrationMatch>> SearchRegistration(string eventAcronym, string searchString, IEnumerable<RegistrationState> states)
        {
            return await _mediator.Send(new SearchRegistrationQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                SearchString = searchString,
                States = states
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}")]
        public async Task<RegistrationDisplayItem> SearchRegistration(string eventAcronym, Guid registrationId)
        {
            return await _mediator.Send(new RegistrationQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/swapFirstLastName")]
        public async Task SwapFirstLastName(string eventAcronym, Guid registrationId)
        {
            await _mediator.Send(new SwapFirstLastNameCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }
    }
}
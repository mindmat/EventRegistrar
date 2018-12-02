using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Spots
{
    public class SpotsController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public SpotsController(IMediator mediator,
                               IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPut("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots/{registrableId:guid}")]
        public async Task AddSpot(string eventAcronym, Guid registrationId, Guid registrableId, bool asFollower)
        {
            await _mediator.Send(new AddSpotCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                RegistrableId = registrableId,
                AsFollower = asFollower
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots")]
        public async Task<IEnumerable<SpotDisplayItem>> GetSpotsOfRegistration(string eventAcronym, Guid registrationId)
        {
            return await _mediator.Send(new SpotsOfRegistrationQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }

        [HttpDelete("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots/{registrableId:guid}")]
        public async Task RemoveSpot(string eventAcronym, Guid registrationId, Guid registrableId)
        {
            await _mediator.Send(new RemoveSpotCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                RegistrableId = registrableId
            });
        }
    }
}
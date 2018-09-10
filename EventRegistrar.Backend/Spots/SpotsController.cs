using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Spots
{
    public class SpotsController : Controller
    {
        private readonly IMediator _mediator;

        public SpotsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots/{registrableId:guid}")]
        public Task AddSpot(string eventAcronym, Guid registrationId, Guid registrableId)
        {
            return _mediator.Send(new AddSpotCommand { EventAcronym = eventAcronym, RegistrationId = registrationId, RegistrableId = registrableId });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots")]
        public Task<IEnumerable<Spot>> GetSpotsOfRegistration(string eventAcronym, Guid registrationId)
        {
            return _mediator.Send(new SpotsOfRegistrationQuery { EventAcronym = eventAcronym, RegistrationId = registrationId });
        }

        [HttpDelete("api/events/{eventAcronym}/registrations/{registrationId:guid}/spots/{registrableId:guid}")]
        public Task RemoveSpot(string eventAcronym, Guid registrationId, Guid registrableId)
        {
            return _mediator.Send(new RemoveSpotCommand { EventAcronym = eventAcronym, RegistrationId = registrationId, RegistrableId = registrableId });
        }
    }
}
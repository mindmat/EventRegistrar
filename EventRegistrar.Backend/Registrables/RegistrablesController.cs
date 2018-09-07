using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrables.Participants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesController : Controller
    {
        private readonly IMediator _mediator;

        public RegistrablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/DoubleRegistrableOverview")]
        public Task<IEnumerable<DoubleRegistrableDisplayItem>> GetDoubleRegistrablesOverivew(string eventAcronym)
        {
            return _mediator.Send(new DoubleRegistrablesOverviewQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/registrables/{registrableId:guid}/participants")]
        public Task<RegistrableDisplayInfo> GetParticipants(string eventAcronym, Guid registrableId)
        {
            return _mediator.Send(new ParticipantsOfRegistrableQuery { EventAcronym = eventAcronym, RegistrableId = registrableId });
        }

        [HttpGet("api/events/{eventAcronym}/registrables")]
        public Task<IEnumerable<RegistrableDisplayItem>> GetRegistrables(string eventAcronym)
        {
            return _mediator.Send(new RegistrablesQuery { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events/{eventAcronym}/SingleRegistrableOverview")]
        public Task<IEnumerable<SingleRegistrableDisplayItem>> GetSingleRegistrablesOverivew(string eventAcronym)
        {
            return _mediator.Send(new SingleRegistrablesOverviewQuery { EventAcronym = eventAcronym });
        }

        [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/limits")]
        public Task SetCoupleLimits(string eventAcronym, Guid registrableId, SetDoubleRegistrationLimitsCommand limits)
        {
            return _mediator.Send(new SetDoubleRegistrationLimitsCommand
            {
                EventAcronym = eventAcronym,
                MaximumCouples = limits.MaximumCouples,
                RegistrableId = registrableId,
                MaximumImbalance = limits.MaximumImbalance
            });
        }
    }
}
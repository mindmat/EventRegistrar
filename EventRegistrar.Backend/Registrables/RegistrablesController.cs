using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Registrables.WaitingList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public RegistrablesController(IMediator mediator,
                                      IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/DoubleRegistrableOverview")]
        public async Task<IEnumerable<DoubleRegistrableDisplayItem>> GetDoubleRegistrablesOverivew(string eventAcronym)
        {
            return await _mediator.Send(new DoubleRegistrablesOverviewQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrables/{registrableId:guid}/participants")]
        public async Task<RegistrableDisplayInfo> GetParticipants(string eventAcronym, Guid registrableId)
        {
            return await _mediator.Send(new ParticipantsOfRegistrableQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrableId = registrableId
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrables")]
        public async Task<IEnumerable<RegistrableDisplayItem>> GetRegistrables(string eventAcronym)
        {
            return await _mediator.Send(new RegistrablesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrationsOnWaitingList")]
        public async Task<IEnumerable<WaitingListSpot>> GetRegistrationsOnWaitingList(string eventAcronym)
        {
            return await _mediator.Send(new RegistrationsOnWaitingListQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpGet("api/events/{eventAcronym}/SingleRegistrableOverview")]
        public async Task<IEnumerable<SingleRegistrableDisplayItem>> GetSingleRegistrablesOverivew(string eventAcronym)
        {
            return await _mediator.Send(new SingleRegistrablesOverviewQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/coupleLimits")]
        public async Task SetCoupleLimits(string eventAcronym, Guid registrableId, [FromBody]SetDoubleRegistrableLimitsCommand limits)
        {
            await _mediator.Send(new SetDoubleRegistrableLimitsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                MaximumCouples = limits.MaximumCouples,
                RegistrableId = registrableId,
                MaximumImbalance = limits.MaximumImbalance
            });
        }

        [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/singleLimits")]
        public async Task SetSingleLimits(string eventAcronym, Guid registrableId, [FromBody]SetSingleRegistrableLimitsCommand limits)
        {
            await _mediator.Send(new SetSingleRegistrableLimitsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                MaximumParticipants = limits.MaximumParticipants,
                RegistrableId = registrableId
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList")]
        public async Task TryPromoteFromWaitingList(string eventAcronym, Guid registrableId)
        {
            await _mediator.Send(new TryPromoteFromWaitingListCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrableId = registrableId
            });
        }

        [HttpDelete("api/events/{eventAcronym}/registrables/{registrableId:guid}")]
        public async Task DeleteRegistrable(string eventAcronym, Guid registrableId)
        {
            await _mediator.Send(new DeleteRegistrableCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrableId = registrableId
            });
        }
    }
}
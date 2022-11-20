using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrables.WaitingList.Promotion;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrables;

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


    [HttpGet("api/events/{eventAcronym}/registrationsOnWaitingList")]
    public async Task<IEnumerable<WaitingListSpot>> GetRegistrationsOnWaitingList(string eventAcronym)
    {
        return await _mediator.Send(new RegistrationsOnWaitingListQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/coupleLimits")]
    public async Task SetCoupleLimits(string eventAcronym,
                                      Guid registrableId,
                                      [FromBody] SetDoubleRegistrableLimitsCommand limits)
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
    public async Task SetSingleLimits(string eventAcronym,
                                      Guid registrableId,
                                      [FromBody] SetSingleRegistrableLimitsCommand limits)
    {
        await _mediator.Send(new SetSingleRegistrableLimitsCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 MaximumParticipants = limits.MaximumParticipants,
                                 RegistrableId = registrableId
                             });
    }

    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList/{registrationId:guid}")]
    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList")]
    public async Task TryPromoteFromWaitingList(string eventAcronym, Guid registrableId, Guid? registrationId)
    {
        await _mediator.Send(new TryPromoteFromWaitingListCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrableId = registrableId,
                                 RegistrationId = registrationId
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


    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/activateAutomaticPromotion")]
    public async Task ActivateAutomaticPromotion(string eventAcronym, Guid registrableId, bool tryPromoteImmediately)
    {
        await _mediator.Send(new ActivateAutomaticPromotionCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrableId = registrableId,
                                 TryPromoteImmediately = tryPromoteImmediately
                             });
    }

    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/deactivateAutomaticPromotion")]
    public async Task DeactivateAutomaticPromotion(string eventAcronym, Guid registrableId)
    {
        await _mediator.Send(new DeactivateAutomaticPromotionCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrableId = registrableId
                             });
    }
}
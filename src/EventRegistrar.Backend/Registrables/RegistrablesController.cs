using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.Registrables.Tags;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrables.WaitingList.Promotion;

using MediatR;

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


    [HttpGet("api/events/{eventAcronym}/registrables")]
    public async Task<IEnumerable<RegistrableDisplayItem>> GetRegistrables(string eventAcronym)
    {
        return await _mediator.Send(new RegistrablesQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
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

    [HttpPost(
        "api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList/{registrationId:guid}")]
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

    [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}")]
    public async Task CreateRegistrable(string eventAcronym,
                                        Guid registrableId,
                                        [FromBody] CreateRegistrableParameters parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        parameters.Id = registrableId;
        await _mediator.Send(new CreateRegistrableCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 Parameters = parameters
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

    [HttpGet("api/events/{eventAcronym}/registrables/pricing")]
    public async Task<IEnumerable<RegistrablePricing>> GetPricing(string eventAcronym)
    {
        return await _mediator.Send(new PricingQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/prices")]
    public async Task SetRegistrablePrices(string eventAcronym,
                                           Guid registrableId,
                                           [FromQuery] decimal? price,
                                           [FromQuery] decimal? reducedPrice)
    {
        await _mediator.Send(new SetRegistrablesPricesCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrableId = registrableId,
                                 Price = price,
                                 ReducedPrice = reducedPrice
                             });
    }


    [HttpPut("api/events/{eventAcronym}/registrables/{registrableId:guid}/reductions/{reductionId:guid}")]
    public async Task SaveReduction(string eventAcronym,
                                    Guid registrableId,
                                    Guid reductionId,
                                    [FromBody] Reduction reduction)
    {
        await _mediator.Send(new SaveReductionCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 ReductionId = reductionId,
                                 RegistrableId = registrableId,
                                 Amount = reduction.Amount,
                                 RegistrableId1_ReductionActivatedIfCombinedWith =
                                     reduction?.RegistrableId1_ReductionActivatedIfCombinedWith,
                                 RegistrableId2_ReductionActivatedIfCombinedWith =
                                     reduction?.RegistrableId2_ReductionActivatedIfCombinedWith
                             });
    }

    [HttpDelete("api/events/{eventAcronym}/registrables/{registrableId:guid}/reductions/{reductionId:guid}")]
    public async Task DeleteReduction(string eventAcronym, Guid registrableId, Guid reductionId)
    {
        await _mediator.Send(new DeleteReductionCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 ReductionId = reductionId,
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
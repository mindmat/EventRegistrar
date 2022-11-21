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
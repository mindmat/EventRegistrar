using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesController(IMediator mediator,
                                    IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList/{registrationId:guid}")]
    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/tryPromoteFromWaitingList")]
    public async Task TryPromoteFromWaitingList(string eventAcronym, Guid registrableId, Guid? registrationId)
    {
        await mediator.Send(new TriggerMoveUpFromWaitingListCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrableId = registrableId,
                                RegistrationId = registrationId
                            });
    }


    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/activateAutomaticPromotion")]
    public async Task ActivateAutomaticPromotion(string eventAcronym, Guid registrableId, bool tryPromoteImmediately)
    {
        await mediator.Send(new ActivateAutomaticPromotionCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrableId = registrableId,
                                TryPromoteImmediately = tryPromoteImmediately
                            });
    }

    [HttpPost("api/events/{eventAcronym}/registrables/{registrableId:guid}/deactivateAutomaticPromotion")]
    public async Task DeactivateAutomaticPromotion(string eventAcronym, Guid registrableId)
    {
        await mediator.Send(new DeactivateAutomaticPromotionCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrableId = registrableId
                            });
    }
}
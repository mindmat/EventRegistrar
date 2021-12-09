using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class IndividualReductionController : Controller
{
    private readonly IEventAcronymResolver _acronymResolver;
    private readonly IMediator _mediator;

    public IndividualReductionController(IMediator mediator,
                                         IEventAcronymResolver acronymResolver)
    {
        _mediator = mediator;
        _acronymResolver = acronymResolver;
    }

    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/reductions/{reductionId:guid}")]
    public async Task AddIndividualReduction(string eventAcronym, Guid registrationId, Guid reductionId, decimal amount,
                                             string reason)
    {
        await _mediator.Send(new AddIndividualReductionCommand
                             {
                                 EventId = await _acronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationId = registrationId,
                                 ReductionId = reductionId,
                                 Amount = amount,
                                 Reason = reason
                             });
    }
}
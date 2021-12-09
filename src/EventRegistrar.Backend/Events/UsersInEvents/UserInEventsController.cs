using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UserInEventsController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public UserInEventsController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpPut("api/events/{eventAcronym}/users/{userId:guid}/roles/{role}")]
    public async Task AddRole(string eventAcronym, Guid userId, UserInEventRole role)
    {
        await _mediator.Send(new AddUserToRoleInEventCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 UserId = userId,
                                 Role = role
                             });
    }

    [HttpGet("api/me/events")]
    public Task<IEnumerable<UserInEventDisplayItem>> GetMyEvents(bool includeRequestedEvents)
    {
        return _mediator.Send(new EventsOfUserQuery { IncludeRequestedEvents = includeRequestedEvents });
    }

    [HttpDelete("api/events/{eventAcronym}/users/{userId:guid}/roles/{role}")]
    public async Task RemoveRole(string eventAcronym, Guid userId, UserInEventRole role)
    {
        await _mediator.Send(new RemoveUserFromRoleInEventCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 UserId = userId,
                                 Role = role
                             });
    }
}
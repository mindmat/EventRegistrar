using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.UsersInEvents;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    [CheckRole(UserInEventRole.Reader)]
    public class SingleRegistrablesOverviewQuery : IRequest<IEnumerable<SingleRegistrableDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
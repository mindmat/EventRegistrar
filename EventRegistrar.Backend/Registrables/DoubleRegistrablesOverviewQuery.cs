using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.UsersInEvents;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    [CheckRole(UserInEventRole.Reader)]
    public class DoubleRegistrablesOverviewQuery : IRequest<IEnumerable<DoubleRegistrableDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
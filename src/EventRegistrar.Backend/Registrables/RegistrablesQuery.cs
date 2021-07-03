using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesQuery : IRequest<IEnumerable<RegistrableDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
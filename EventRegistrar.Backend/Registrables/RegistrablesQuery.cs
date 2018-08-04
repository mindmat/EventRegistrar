using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesQuery : IRequest<IEnumerable<RegistrableItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
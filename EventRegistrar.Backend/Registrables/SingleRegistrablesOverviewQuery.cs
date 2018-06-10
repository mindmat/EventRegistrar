using MediatR;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Registrables
{
    public class SingleRegistrablesOverviewQuery : IRequest<IEnumerable<SingleRegistrableDisplayItem>>
    {
        public string EventAcronym { get; set; }
    }
}
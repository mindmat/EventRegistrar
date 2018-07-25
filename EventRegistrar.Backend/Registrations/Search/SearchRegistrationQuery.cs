using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class SearchRegistrationQuery : IRequest<IEnumerable<RegistrationMatch>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public string SearchString { get; set; }
    }
}
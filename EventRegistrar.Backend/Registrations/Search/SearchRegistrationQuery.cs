using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class SearchRegistrationQuery : IRequest<IEnumerable<RegistrationMatch>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public string SearchString { get; set; }
        public IEnumerable<RegistrationState> States { get; set; }
    }
}
using System.Collections.Generic;
using MediatR;

namespace EventRegistrar.Backend.Events
{
    public class SearchEventQuery : IRequest<IEnumerable<EventSearchResult>>
    {
        public bool IncludeAuthorizedEvents { get; set; }
        public bool IncludeRequestedEvents { get; set; }
        public string SearchString { get; set; }
    }
}
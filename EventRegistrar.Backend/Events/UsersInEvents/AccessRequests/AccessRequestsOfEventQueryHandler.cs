using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class AccessRequestsOfEventQueryHandler : IRequestHandler<AccessRequestsOfEventQuery, IEnumerable<AccessRequestOfEvent>>
    {
        private readonly IQueryable<AccessToEventRequest> _accessRequests;
        private readonly IEventAcronymResolver _acronymResolver;

        public AccessRequestsOfEventQueryHandler(IEventAcronymResolver acronymResolver,
                                                 IQueryable<AccessToEventRequest> accessRequests)
        {
            _acronymResolver = acronymResolver;
            _accessRequests = accessRequests;
        }

        public async Task<IEnumerable<AccessRequestOfEvent>> Handle(AccessRequestsOfEventQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            return _accessRequests.Where(req => req.EventId == eventId)
                                  .Where(req => !req.Response.HasValue
                                              || req.Response == RequestResponse.Denied && query.IncludeDeniedRequests)
                                  .Select(req => new AccessRequestOfEvent
                                  {
                                      Id = req.Id,
                                      FirstName = req.FirstName,
                                      LastName = req.LastName,
                                      Email = req.Email,
                                      RequestReceived = req.RequestReceived,
                                      RequestText = req.RequestText
                                  });
        }
    }
}
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class
    AccessRequestsOfEventQueryHandler : IRequestHandler<AccessRequestsOfEventQuery, IEnumerable<AccessRequestOfEvent>>
{
    private readonly IQueryable<AccessToEventRequest> _accessRequests;

    public AccessRequestsOfEventQueryHandler(IQueryable<AccessToEventRequest> accessRequests)
    {
        _accessRequests = accessRequests;
    }

    public async Task<IEnumerable<AccessRequestOfEvent>> Handle(AccessRequestsOfEventQuery query,
                                                                CancellationToken cancellationToken)
    {
        return await _accessRequests.Where(req => req.EventId == query.EventId)
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
                                                   })
                                    .ToListAsync(cancellationToken);
    }
}
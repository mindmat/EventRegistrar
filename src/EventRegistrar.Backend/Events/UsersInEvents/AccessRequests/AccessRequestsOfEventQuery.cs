namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class AccessRequestsOfEventQuery : IRequest<IEnumerable<AccessRequestOfEvent>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool IncludeDeniedRequests { get; set; }
}

public class AccessRequestOfEvent
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTimeOffset RequestReceived { get; set; }
    public string? RequestText { get; set; }
}

public class AccessRequestsOfEventQueryHandler(IQueryable<AccessToEventRequest> accessRequests) : IRequestHandler<AccessRequestsOfEventQuery, IEnumerable<AccessRequestOfEvent>>
{
    public async Task<IEnumerable<AccessRequestOfEvent>> Handle(AccessRequestsOfEventQuery query,
                                                                CancellationToken cancellationToken)
    {
        return await accessRequests.Where(req => req.EventId == query.EventId)
                                   .Where(req => req.Response == null
                                              || (req.Response == RequestResponse.Denied && query.IncludeDeniedRequests))
                                   .Select(req => new AccessRequestOfEvent
                                                  {
                                                      Id = req.Id,
                                                      FirstName = req.FirstName,
                                                      LastName = req.LastName,
                                                      Email = req.Email,
                                                      AvatarUrl = req.AvatarUrl,
                                                      RequestReceived = req.RequestReceived,
                                                      RequestText = req.RequestText
                                                  })
                                   .ToListAsync(cancellationToken);
    }
}
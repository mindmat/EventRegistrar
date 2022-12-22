using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class RemarksOverviewQuery : IRequest<IEnumerable<RemarksDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool OnlyUnprocessed { get; set; }
}

public class RemarksOverviewQueryHandler : IRequestHandler<RemarksOverviewQuery, IEnumerable<RemarksDisplayItem>>
{
    private readonly IQueryable<Registration> _registrations;

    public RemarksOverviewQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<IEnumerable<RemarksDisplayItem>> Handle(RemarksOverviewQuery query, CancellationToken cancellationToken)
    {
        return await _registrations.Where(reg => reg.EventId == query.EventId
                                              && reg.Remarks != null
                                              && reg.Remarks != string.Empty)
                                   .WhereIf(query.OnlyUnprocessed, reg => !reg.RemarksProcessed)
                                   .OrderByDescending(reg => reg.ReceivedAt)
                                   .Select(reg => new RemarksDisplayItem
                                                  {
                                                      RegistrationId = reg.Id,
                                                      DisplayName = $"{reg.RespondentFirstName} {reg.RespondentLastName}",
                                                      Email = reg.RespondentEmail,
                                                      Remarks = reg.Remarks!,
                                                      Processed = reg.RemarksProcessed
                                                  })
                                   .ToListAsync(cancellationToken);
    }
}

public class RemarksDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string Remarks { get; set; } = null!;
    public bool Processed { get; set; }
}
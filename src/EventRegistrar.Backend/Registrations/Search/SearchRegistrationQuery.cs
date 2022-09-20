using EventRegistrar.Backend.Authorization;

namespace EventRegistrar.Backend.Registrations.Search;

public class SearchRegistrationQuery : IRequest<IEnumerable<RegistrationMatch>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? SearchString { get; set; }
    public IEnumerable<RegistrationState>? States { get; set; }
}

public class SearchRegistrationQueryHandler : IRequestHandler<SearchRegistrationQuery, IEnumerable<RegistrationMatch>>
{
    private readonly IQueryable<Registration> _registrations;

    public SearchRegistrationQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<IEnumerable<RegistrationMatch>> Handle(SearchRegistrationQuery query,
                                                             CancellationToken cancellationToken)
    {
        var allowedStates = query.States?.Any() == true ? query.States : new[] { RegistrationState.Received };
        var searchString = query.SearchString?.Trim();
        if (searchString == null)
        {
            return Enumerable.Empty<RegistrationMatch>();
        }

        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId)
                                                .Where(reg => reg.RespondentFirstName!.Contains(searchString)
                                                           || reg.RespondentLastName!.Contains(searchString)
                                                           || reg.RespondentEmail!.Contains(searchString)
                                                           || reg.PhoneNormalized!.Contains(searchString))
                                                .Where(reg => allowedStates.Contains(reg.State))
                                                .Select(reg => new RegistrationMatch
                                                               {
                                                                   RegistrationId = reg.Id,
                                                                   FirstName = reg.RespondentFirstName,
                                                                   LastName = reg.RespondentLastName,
                                                                   Email = reg.RespondentEmail,
                                                                   ReceivedAt = reg.ReceivedAt,
                                                                   Amount = reg.Price ?? 0m,
                                                                   AmountPaid = reg.PaymentAssignments!.Select(asn =>
                                                                                                                   asn.PayoutRequestId == null
                                                                                                                       ? asn.Amount
                                                                                                                       : -asn.Amount)
                                                                                   .Sum(),
                                                                   State = reg.State,
                                                                   StateText = reg.State.ToString(),
                                                                   IsWaitingList = reg.IsWaitingList == true,
                                                                   Spots = reg.Seats_AsLeader!
                                                                              .Where(spt => !spt.IsCancelled)
                                                                              .Select(spt => new SpotShort
                                                                                             {
                                                                                                 Name = spt.Registrable!.Name,
                                                                                                 Secondary = spt.Registrable.NameSecondary,
                                                                                                 IsWaitingList = spt.IsWaitingList
                                                                                             })
                                                                              .Union(reg.Seats_AsFollower!
                                                                                        .Where(spt => !spt.IsCancelled)
                                                                                        .Select(spt => new SpotShort
                                                                                                       {
                                                                                                           Name = spt.Registrable!.Name,
                                                                                                           Secondary = spt.Registrable.NameSecondary,
                                                                                                           IsWaitingList = spt.IsWaitingList
                                                                                                       }))
                                                               })
                                                .OrderBy(reg => reg.FirstName)
                                                .ThenBy(reg => reg.LastName)
                                                .Take(20)
                                                .ToListAsync(cancellationToken);
        return registrations;
    }
}

public class RegistrationMatch
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsWaitingList { get; set; }
    public RegistrationState State { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public string StateText { get; set; }
    public IEnumerable<SpotShort> Spots { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}

public class SpotShort
{
    public string Name { get; set; } = null!;
    public string? Secondary { get; set; }
    public bool IsWaitingList { get; set; }
}
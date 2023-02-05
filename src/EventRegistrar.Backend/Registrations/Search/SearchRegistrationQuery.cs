using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

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
    private readonly ReadModelReader _readModelReader;

    public SearchRegistrationQueryHandler(IQueryable<Registration> registrations,
                                          ReadModelReader readModelReader)
    {
        _registrations = registrations;
        _readModelReader = readModelReader;
    }

    public async Task<IEnumerable<RegistrationMatch>> Handle(SearchRegistrationQuery query,
                                                             CancellationToken cancellationToken)
    {
        var allowedStates = query.States?.Any() == true
                                ? query.States
                                : new[] { RegistrationState.Received, RegistrationState.Paid };
        var searchParts = query.SearchString?.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        var queryable = _registrations.Where(reg => reg.EventId == query.EventId);
        if (searchParts != null)
        {
            foreach (var searchPart in searchParts)
            {
                queryable = queryable.Where(reg => reg.RespondentFirstName!.Contains(searchPart)
                                                || reg.RespondentLastName!.Contains(searchPart)
                                                || reg.RespondentEmail!.Contains(searchPart)
                                                || reg.PhoneNormalized!.Contains(searchPart));
            }
        }

        var registrationIds = await queryable.Where(reg => allowedStates.Contains(reg.State))
                                             .OrderBy(reg => reg.RespondentFirstName)
                                             .ThenBy(reg => reg.RespondentLastName)
                                             .Take(21)
                                             .Select(reg => reg.Id)
                                             .ToListAsync(cancellationToken);

        var registrations = await _readModelReader.GetDeserialized<RegistrationDisplayItem>(nameof(RegistrationQuery), query.EventId, registrationIds, cancellationToken);

        return registrations.Select(reg => new RegistrationMatch
                                           {
                                               RegistrationId = reg.Id,
                                               FirstName = reg.FirstName,
                                               LastName = reg.LastName,
                                               Email = reg.Email,
                                               ReceivedAt = reg.ReceivedAt,
                                               Price = reg.Price ?? 0m,
                                               AmountPaid = reg.Paid,
                                               State = reg.Status,
                                               StateText = reg.Status.ToString(),
                                               IsWaitingList = reg.IsWaitingList == true,
                                               Spots = reg.Spots!.Select(spt => new SpotShort
                                                                                {
                                                                                    Name = spt.RegistrableName,
                                                                                    Secondary = spt.RegistrableNameSecondary,
                                                                                    IsWaitingList = spt.IsWaitingList
                                                                                })
                                           })
                            .ToList();
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
    public decimal Price { get; set; }
    public decimal AmountPaid { get; set; }
    public string StateText { get; set; }
    public IEnumerable<SpotShort> Spots { get; set; } = null!;
    public DateTimeOffset ReceivedAt { get; set; }
}

public class SpotShort
{
    public string Name { get; set; } = null!;
    public string? Secondary { get; set; }
    public bool IsWaitingList { get; set; }
}
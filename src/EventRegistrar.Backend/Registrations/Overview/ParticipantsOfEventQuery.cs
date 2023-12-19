using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Pricing;

namespace EventRegistrar.Backend.Registrations.Overview;

public class ParticipantsOfEventQuery : IRequest<IEnumerable<Participant>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? SearchString { get; set; }
    public string? Tag { get; set; }
    public bool IncludeWaitingList { get; set; }
    public IEnumerable<RegistrationState>? States { get; set; }
}

public class Participant
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PricePackageAdmitted { get; set; }
    public bool IsOnWaitingList { get; set; }
    public RegistrationState State { get; set; }
    public string CoreSpots { get; set; } = null!;
    public string StateText { get; set; } = null!;
    public decimal AmountOutstanding { get; set; }
}

public class ParticipantsOfEventQueryHandler(IQueryable<Registration> _registrations,
                                             IQueryable<PricePackage> pricePackages,
                                             EnumTranslator enumTranslator,
                                             ReadModelReader readModelReader,
                                             IQueryable<Registrable> tracks)
    : IRequestHandler<ParticipantsOfEventQuery, IEnumerable<Participant>>
{
    public async Task<IEnumerable<Participant>> Handle(ParticipantsOfEventQuery query, CancellationToken cancellationToken)
    {
        var allowedStates = query.States?.Any() == true
                                ? query.States
                                : new[] { RegistrationState.Received, RegistrationState.Paid };
        var searchParts = query.SearchString?.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var packages = await pricePackages.Where(pkg => pkg.EventId == query.EventId)
                                          .ToDictionaryAsync(pkg => pkg.Id, pkg => pkg.Name, cancellationToken);

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

        var registrationIds = await queryable.Where(reg => allowedStates.Contains(reg.State)
                                                        && reg.IsOnWaitingList == query.IncludeWaitingList)
                                             .OrderBy(reg => reg.RespondentFirstName)
                                             .ThenBy(reg => reg.RespondentLastName)
                                             .Select(reg => new { reg.Id, reg.PricePackageIds_Admitted })
                                             .ToListAsync(cancellationToken);

        var registrations = await readModelReader.GetDeserialized<RegistrationDisplayItem>(nameof(RegistrationQuery), query.EventId, registrationIds.Select(reg => reg.Id), cancellationToken);
        var registrableIds = await tracks.Where(trk => trk.IsCore && trk.CheckinListColumn == "Tracks")
                                         //.WhereIf(!string.IsNullOrWhiteSpace(query.Tag), trk => trk.Tag == query.Tag)
                                         .Select(trk => trk.Id)
                                         .ToListAsync(cancellationToken);
        return registrations.Select(reg => new Participant
                                           {
                                               RegistrationId = reg.Id,
                                               FirstName = reg.FirstName,
                                               LastName = reg.LastName,
                                               Email = reg.Email,
                                               AmountOutstanding = (reg.Price ?? 0m) - reg.Paid,
                                               State = reg.Status,
                                               StateText = enumTranslator.Translate(reg.Status),
                                               IsOnWaitingList = reg.IsWaitingList == true,
                                               CoreSpots = reg.Spots!
                                                              .Where(spt => !spt.IsWaitingList && registrableIds.Contains(spt.RegistrableId))
                                                              .Select(spt => $"{spt.RegistrableName} {spt.RegistrableNameSecondary}")
                                                              .StringJoin(),
                                               PricePackageAdmitted = GetPricePackageText(registrationIds.First(r => r.Id == reg.Id).PricePackageIds_Admitted, packages)
                                           })
                            .OrderBy(reg => reg.FirstName)
                            .ThenBy(reg => reg.LastName)
                            .ToList();
    }

    private static string? GetPricePackageText(IEnumerable<Guid> pricePackageIds, IReadOnlyDictionary<Guid, string> packages)
    {
        var pricePackageId = pricePackageIds.FirstOrDefault();
        return pricePackageId != default && packages.TryGetValue(pricePackageId, out var packageText)
                   ? packageText
                   : null;
    }
}
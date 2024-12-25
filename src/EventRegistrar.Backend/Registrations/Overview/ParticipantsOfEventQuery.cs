using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;
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
    public bool AddDetails { get; set; }
}

public class Participant : IDynamicColumns
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PricePackageAdmitted { get; set; }
    public bool IsOnWaitingList { get; set; }
    public RegistrationState State { get; set; }
    public string CoreSpots { get; set; } = null!;
    public string StateText { get; set; } = null!;
    public decimal AmountOutstanding { get; set; }
    public bool IsVolunteer { get; set; }
    public string? Location { get; set; }
    public IDictionary<string, string>? DynamicColumns { get; set; }
}

public class ParticipantsOfEventQueryHandler(IQueryable<Registration> _registrations,
                                             IQueryable<PricePackage> pricePackages,
                                             IQueryable<Registrable> _registrables,
                                             EnumTranslator enumTranslator,
                                             ReadModelReader readModelReader,
                                             IQueryable<Registrable> tracks)
    : IRequestHandler<ParticipantsOfEventQuery, IEnumerable<Participant>>
{
    public async Task<IEnumerable<Participant>> Handle(ParticipantsOfEventQuery query, CancellationToken cancellationToken)
    {
        var dynamicColumns = query.AddDetails
                                 ? (await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                                  && rbl.CheckinListColumn != null)
                                                       .Select(rbl => new { rbl.Id, rbl.DisplayName, rbl.CheckinListColumn })
                                                       .ToListAsync(cancellationToken))
                                   .GroupBy(rbl => rbl.CheckinListColumn!)
                                   .ToDictionary(grp => grp.Key,
                                                 grp => grp.Select(rbl => new { rbl.Id, rbl.DisplayName }))
                                 : null;

        var allowedStates = query.States?.Any() == true
                                ? query.States
                                : [RegistrationState.Received, RegistrationState.Paid];
        var searchParts = query.SearchString?.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var packages = await pricePackages.Where(pkg => pkg.EventId == query.EventId)
                                          .ToDictionaryAsync(pkg => pkg.Id, pkg => pkg.Name, cancellationToken);

        var queryable = _registrations.Where(reg => reg.EventId == query.EventId);
        if (searchParts?.Any() == true)
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
                                             .WhereIf(!query.IncludeWaitingList, reg => reg.IsOnWaitingList == false)
                                             .OrderBy(reg => reg.RespondentFirstName)
                                             .ThenBy(reg => reg.RespondentLastName)
                                             .Select(reg => new { reg.Id, reg.PricePackageIds_Admitted })
                                             .ToListAsync(cancellationToken);

        var registrations = await readModelReader.GetDeserialized<RegistrationDisplayItem>(nameof(RegistrationQuery), query.EventId, registrationIds.Select(reg => reg.Id), cancellationToken);
        var registrables = await tracks.Where(trk => trk.EventId == query.EventId)
                                       .Select(trk => new
                                                      {
                                                          trk.Id,
                                                          IsCoreTrack = trk.IsCore && trk.CheckinListColumn == "Tracks"
                                                      })
                                       .Where(trk => trk.IsCoreTrack)
                                       .ToListAsync(cancellationToken);
        var registrableIds = registrables.Where(trk => trk.IsCoreTrack)
                                         .Select(trk => trk.Id)
                                         .ToList();
        return registrations.Select(reg => new Participant
                                           {
                                               RegistrationId = reg.Id,
                                               FirstName = reg.FirstName,
                                               LastName = reg.LastName,
                                               Email = reg.Email,
                                               Phone = reg.PhoneNormalized,
                                               AmountOutstanding = (reg.Price ?? 0m) - reg.Paid,
                                               State = reg.Status,
                                               StateText = enumTranslator.Translate(reg.Status),
                                               IsOnWaitingList = reg.IsWaitingList == true,
                                               CoreSpots = reg.Spots!
                                                              .Where(spt => !spt.IsWaitingList && registrableIds.Contains(spt.RegistrableId))
                                                              .Select(spt => GetSpotText(spt.RegistrableName, spt.RegistrableNameSecondary, spt.RoleText))
                                                              .StringJoin(),
                                               PricePackageAdmitted = GetPricePackageText(registrationIds.First(r => r.Id == reg.Id).PricePackageIds_Admitted, packages),
                                               Location = reg.Location,
                                               DynamicColumns = dynamicColumns?.ToDictionary(col => col.Key,
                                                                                             col => col.Value.Where(rbl => reg.Spots!.Any(spt => spt.RegistrableId == rbl.Id))
                                                                                                       .Select(rbl => rbl.DisplayName)
                                                                                                       .StringJoin())
                                           })
                            .OrderBy(reg => reg.FirstName)
                            .ThenBy(reg => reg.LastName)
                            .ToList();
    }

    private static string GetSpotText(string registrableName, string? registrableNameSecondary, string? roleText)
    {
        var text = new[] { registrableName, registrableNameSecondary }.StringJoinNullable(" - ")!;
        if (roleText != null)
        {
            text += $" ({roleText})";
        }

        return text;
    }

    private static string? GetPricePackageText(IEnumerable<Guid>? pricePackageIds, IReadOnlyDictionary<Guid, string> packages)
    {
        return pricePackageIds?.Select(packages.GetValueOrDefault).StringJoinNullable();
    }
}
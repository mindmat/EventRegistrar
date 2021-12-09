using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Compositions;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Overview;

public class PartyDetailItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Participants { get; set; }
    public int Potential { get; set; }
}

public class PartyItem
{
    public IEnumerable<PartyDetailItem> Details { get; set; }
    public int Direct { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int PartyPassFallbacksOnWaitingList { get; set; }
    public int Potential { get; set; }
    public int PotentialOnWaitingList { get; set; }
    public int? SortyKey { get; set; }
    public int Total { get; set; }
}

public class PartyOverviewQuery : IRequest<IEnumerable<PartyItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PartyOverviewQueryHandler : IRequestHandler<PartyOverviewQuery, IEnumerable<PartyItem>>
{
    private readonly IQueryable<RegistrableComposition> _compositions;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Registration> _registrations;

    public PartyOverviewQueryHandler(IQueryable<RegistrableComposition> compositions,
                                     IQueryable<Registrable> registrables,
                                     IQueryable<Registration> registrations)
    {
        _compositions = compositions;
        _registrables = registrables;
        _registrations = registrations;
    }

    public async Task<IEnumerable<PartyItem>> Handle(PartyOverviewQuery query, CancellationToken cancellationToken)
    {
        var mappings = (await _compositions.Where(cmp => cmp.Registrable.EventId == query.EventId)
                                           .Select(cmp => new { cmp.RegistrableId_Contains, cmp.RegistrableId })
                                           .ToListAsync(cancellationToken)
                       )
                       .GroupBy(cmp => cmp.RegistrableId_Contains)
                       .Select(grp => new
                                      {
                                          PartyId = grp.Key,
                                          DependentRegistrablesIds = grp.Select(rbl => rbl.RegistrableId)
                                      })
                       .ToList();

        var registrableIdsPartyParticipants = mappings.Select(map => map.PartyId).ToList();
        registrableIdsPartyParticipants.AddRange(mappings.SelectMany(map => map.DependentRegistrablesIds));

        var registrationsOnWaitingList = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                        && reg.IsWaitingList == true
                                                                        && reg.State == RegistrationState.Received)
                                                             .ToListAsync(cancellationToken);
        var partyPassFallbacksOnWaitingList = registrationsOnWaitingList.Count(reg => reg.FallbackToPartyPass == true);
        var registrationsOnWaitinglist = registrationsOnWaitingList.Count();

        //log.Info(string.Join(",", idsOfInterest));

        var participants = await _registrables.Where(rbl => registrableIdsPartyParticipants.Contains(rbl.Id))
                                              .Select(rbl => new
                                                             {
                                                                 rbl.Id,
                                                                 rbl.Name,
                                                                 rbl.ShowInMailListOrder,
                                                                 Participants = rbl.Spots.Where(spt =>
                                                                         !spt.IsCancelled && !spt.IsWaitingList)
                                                                     .Select(spt =>
                                                                         (spt.RegistrationId.HasValue ? 1 : 0) +
                                                                         (spt.RegistrationId_Follower.HasValue ? 1 : 0))
                                                                     .Sum(),
                                                                 Potential = rbl.MaximumSingleSeats ??
                                                                             (rbl.MaximumDoubleSeats ?? 0) * 2
                                                             })
                                              .ToDictionaryAsync(rbl => rbl.Id, cancellationToken);

        var overview = mappings.Select(map => new PartyItem
                                              {
                                                  Id = map.PartyId,
                                                  Name = participants[map.PartyId].Name,
                                                  SortyKey = participants[map.PartyId].ShowInMailListOrder,
                                                  Direct = participants[map.PartyId].Participants,
                                                  Total = participants[map.PartyId].Participants
                                                        + participants.Values.Where(rbl =>
                                                                          map.DependentRegistrablesIds.Contains(rbl.Id))
                                                                      .Sum(rbl => rbl.Participants)
                                                        + partyPassFallbacksOnWaitingList,
                                                  Potential = participants
                                                              .Values.Where(rbl =>
                                                                  map.DependentRegistrablesIds.Contains(rbl.Id))
                                                              .Sum(rbl => Math.Max(0,
                                                                  rbl.Potential - rbl.Participants)),
                                                  PartyPassFallbacksOnWaitingList = partyPassFallbacksOnWaitingList,
                                                  PotentialOnWaitingList = registrationsOnWaitinglist -
                                                                           partyPassFallbacksOnWaitingList,
                                                  Details = participants
                                                            .Values.Where(rbl =>
                                                                map.DependentRegistrablesIds.Contains(rbl.Id))
                                                            .Select(rbl => new PartyDetailItem
                                                                           {
                                                                               Id = rbl.Id,
                                                                               Name = rbl.Name,
                                                                               Participants = rbl.Participants,
                                                                               Potential = rbl.Potential -
                                                                                   rbl.Participants
                                                                           })
                                              })
                               .OrderBy(map => map.SortyKey);

        return overview;
    }
}
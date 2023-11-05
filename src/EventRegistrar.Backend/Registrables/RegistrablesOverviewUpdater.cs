using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables.Tags;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.Register;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesOverviewCalculator : ReadModelCalculator<RegistrablesOverview>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<RegistrableTag> _tags;
    private readonly IAuthorizationChecker _authorizationChecker;

    public RegistrablesOverviewCalculator(IQueryable<Registration> registrations,
                                          IQueryable<Registrable> registrables,
                                          IQueryable<RegistrableTag> tags,
                                          IAuthorizationChecker authorizationChecker)
    {
        _registrations = registrations;
        _registrables = registrables;
        _tags = tags;
        _authorizationChecker = authorizationChecker;
    }

    public override string QueryName => nameof(RegistrablesOverviewQuery);
    public override bool IsDateDependent => false;

    public override async Task<RegistrablesOverview> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == eventId)
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration_Follower)
                                              .Include(rbl => rbl.Event)
                                              .ToListAsync(cancellationToken);

        var registrationsOnWaitingList = new HashSet<Guid>(_registrations.Where(reg => reg.EventId == eventId
                                                                                    && reg.IsOnWaitingList == true)
                                                                         .Select(reg => reg.Id));

        var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(eventId, nameof(DeleteRegistrableCommand));
        var tags = await _tags.Where(tag => tag.EventId == eventId)
                              .ToDictionaryAsync(tag => tag.Tag,
                                                 tag => tag.SortKey,
                                                 cancellationToken);
        return new RegistrablesOverview
               {
                   SingleRegistrables = registrables.Where(rbl => rbl.MaximumDoubleSeats == null)
                                                    .OrderBy(rbl => rbl.Tag != null && tags.TryGetValue(rbl.Tag, out var sortKey) ? sortKey : int.MaxValue)
                                                    .ThenBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                    .Select(rbl => new SingleRegistrableDisplayItem
                                                                   {
                                                                       Id = rbl.Id,
                                                                       Name = rbl.Name,
                                                                       NameSecondary = rbl.NameSecondary,
                                                                       Tag = rbl.Tag,
                                                                       SpotsAvailable = rbl.MaximumSingleSeats,
                                                                       HasWaitingList = rbl.HasWaitingList,
                                                                       AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                                                                       Accepted = rbl.Spots!.Count(spt => spt is { IsCancelled: false, IsWaitingList: false }
                                                                                                       && !registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty)),
                                                                       OnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                            && (spt.IsWaitingList
                                                                                                             || registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty))),
                                                                       IsDeletable = !rbl.Spots!.Any(spt => !spt.IsCancelled)
                                                                                  && userCanDeleteRegistrable
                                                                                  && rbl.Event!.State == RegistrationForms.EventState.Setup,
                                                                       Class = rbl.Spots!.Where(spt => spt is { IsCancelled: false, IsWaitingList: false })
                                                                                  .Select(GetSpotState)
                                                                                  .FillUpIf(rbl.MaximumSingleSeats, () => SpotState.Available)
                                                                                  .ToList(),
                                                                       WaitingList = rbl.Spots!.Where(spt => spt is { IsCancelled: false, IsWaitingList: true })
                                                                                        .Select(GetSpotState)
                                                                                        .ToList()
                                                                   }),
                   DoubleRegistrables = registrables.Where(rbl => rbl.MaximumDoubleSeats != null)
                                                    .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                    .Select(rbl => new DoubleRegistrableDisplayItem
                                                                   {
                                                                       Id = rbl.Id,
                                                                       Name = rbl.Name,
                                                                       NameSecondary = rbl.NameSecondary,
                                                                       Tag = rbl.Tag,
                                                                       SpotsAvailable = rbl.MaximumDoubleSeats,
                                                                       HasWaitingList = rbl.HasWaitingList,
                                                                       AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                                                                       MaximumAllowedImbalance = rbl.MaximumAllowedImbalance,
                                                                       LeadersAccepted = rbl.Spots!.Count(spt => spt is { IsCancelled: false, IsWaitingList: false, RegistrationId: not null }),
                                                                       FollowersAccepted = rbl.Spots!.Count(
                                                                           spt => spt is { IsCancelled: false, IsWaitingList: false, RegistrationId_Follower: not null }),
                                                                       LeadersOnWaitingList = rbl.Spots!.Count(spt => spt is { IsCancelled: false, IsWaitingList: true }
                                                                                                                   && spt.IsSingleLeaderSpot()),
                                                                       FollowersOnWaitingList = rbl.Spots!.Count(spt => spt is { IsCancelled: false, IsWaitingList: true }
                                                                                                                     && spt.IsSingleFollowerSpot()),
                                                                       CouplesOnWaitingList = rbl.Spots!.Count(spt => spt is { IsCancelled: false, IsWaitingList: true }
                                                                                                                   && (spt.IsUnmatchedPartnerSpot() || spt.IsMatchedPartnerSpot())),
                                                                       IsDeletable = !rbl.Spots!.Any(spt => !spt.IsCancelled)
                                                                                  && userCanDeleteRegistrable
                                                                                  && rbl.Event!.State == RegistrationForms.EventState.Setup,
                                                                       Class = rbl.Spots!.Where(spt => spt is { IsCancelled: false, IsWaitingList: false })
                                                                                  .OrderBy(spt => spt.FirstPartnerJoined)
                                                                                  .Select(GetDoubleSpotState)
                                                                                  .FillUpIf(rbl.MaximumDoubleSeats,
                                                                                            () => new DoubleSpotState { Leader = SpotState.Available, Follower = SpotState.Available })
                                                                                  .ToList(),
                                                                       WaitingList = rbl.Spots!.Where(spt => spt is { IsCancelled: false, IsWaitingList: true })
                                                                                        .OrderBy(spt => spt.FirstPartnerJoined)
                                                                                        .Select(GetDoubleSpotState)
                                                                                        .ToList()
                                                                   })
                                                    .ToList()
               };
    }

    private static SpotState GetSpotState(Seat spot)
    {
        return spot.Registration == null
                   ? SpotState.Available
                   : spot.Registration.State == RegistrationState.Paid
                       ? SpotState.Paid
                       : SpotState.Registered;
    }

    private static DoubleSpotState GetDoubleSpotState(Seat spot)
    {
        return new DoubleSpotState
               {
                   Leader = spot.Registration == null
                                ? spot.IsPartnerSpot
                                      ? SpotState.Reserved
                                      : SpotState.Available
                                : spot.Registration.State == RegistrationState.Paid
                                    ? SpotState.Paid
                                    : SpotState.Registered,
                   Follower = spot.Registration_Follower == null
                                  ? spot.IsPartnerSpot
                                        ? SpotState.Reserved
                                        : SpotState.Available
                                  : spot.Registration_Follower.State == RegistrationState.Paid
                                      ? SpotState.Paid
                                      : SpotState.Registered,
                   Linked = spot.IsPartnerSpot
               };
    }
}

public class UpdateTrackOverviewReadModel : IEventToCommandTranslation<RegistrationProcessed>,
                                            IEventToCommandTranslation<RegistrationCancelled>,
                                            IEventToCommandTranslation<SpotAdded>,
                                            IEventToCommandTranslation<SpotRemoved>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTrackOverviewReadModel(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public IEnumerable<IRequest> Translate(RegistrationProcessed e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId.Value);
        }
    }


    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId.Value);
        }
    }

    public IEnumerable<IRequest> Translate(SpotAdded e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId.Value);
        }
    }

    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId.Value);
        }
    }

    private UpdateReadModelCommand CreateUpdateCommand(Guid eventId)
    {
        return new UpdateReadModelCommand
               {
                   QueryName = nameof(RegistrablesOverviewQuery),
                   EventId = eventId,
                   DirtyMoment = _dateTimeProvider.Now
               };
    }
}

public class SingleRegistrableDisplayItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; }
    public string? Tag { get; set; }
    public int Accepted { get; set; }
    public int? OnWaitingList { get; set; }
    public int? SpotsAvailable { get; set; }
    public bool HasWaitingList { get; set; }
    public bool IsDeletable { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public IEnumerable<SpotState> Class { get; set; } = null!;
    public IEnumerable<SpotState> WaitingList { get; set; } = null!;
}

public record DoubleRegistrableDisplayItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; }
    public string? Tag { get; set; }
    public int CouplesOnWaitingList { get; set; }
    public int FollowersAccepted { get; set; }
    public int FollowersOnWaitingList { get; set; }
    public int LeadersAccepted { get; set; }
    public int LeadersOnWaitingList { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public int? SpotsAvailable { get; set; }
    public bool HasWaitingList { get; set; }
    public bool IsDeletable { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public IEnumerable<DoubleSpotState> Class { get; set; } = null!;
    public IEnumerable<DoubleSpotState> WaitingList { get; set; } = null!;
}

public enum SpotState
{
    Available = 1,
    Reserved = 2,
    Registered = 3,
    Paid = 4
}

public record DoubleSpotState
{
    public SpotState Leader { get; set; }
    public SpotState Follower { get; set; }
    public bool Linked { get; set; }
}
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Register;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesOverviewUpdater : ReadModelUpdater<RegistrablesOverview>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IAuthorizationChecker _authorizationChecker;

    public RegistrablesOverviewUpdater(IQueryable<Registration> registrations,
                                       IQueryable<Registrable> registrables,
                                       IAuthorizationChecker authorizationChecker)
    {
        _registrations = registrations;
        _registrables = registrables;
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
                                                                                    && reg.IsWaitingList == true)
                                                                         .Select(reg => reg.Id));

        var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(eventId, nameof(DeleteRegistrableCommand));

        return new RegistrablesOverview
               {
                   SingleRegistrables = registrables.Where(rbl => rbl.MaximumDoubleSeats == null)
                                                    .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                    .Select(rbl => new SingleRegistrableDisplayItem
                                                                   {
                                                                       Id = rbl.Id,
                                                                       Name = rbl.Name,
                                                                       NameSecondary = rbl.NameSecondary,
                                                                       Tag = rbl.Tag,
                                                                       SpotsAvailable = rbl.MaximumSingleSeats,
                                                                       HasWaitingList = rbl.HasWaitingList,
                                                                       AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                                                                       Accepted = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                       && !spt.IsWaitingList
                                                                                                       && !registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty)),
                                                                       OnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                            && (spt.IsWaitingList
                                                                                                             || registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty))),
                                                                       IsDeletable = !rbl.Spots!.Any(spt => !spt.IsCancelled)
                                                                                  && userCanDeleteRegistrable
                                                                                  && rbl.Event!.State == RegistrationForms.EventState.Setup,
                                                                       Class = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                                    && !spt.IsWaitingList)
                                                                                  .Select(GetSpotState)
                                                                                  .FillUpIf(rbl.MaximumSingleSeats, () => SpotState.Available)
                                                                                  .ToList(),
                                                                       WaitingList = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                                          && spt.IsWaitingList)
                                                                                        .Select(GetSpotState)
                                                                                        .ToList()
                                                                   }),
                   DoubleRegistrables = registrables.Where(rbl => rbl.MaximumDoubleSeats != null)
                                                    .Select(rbl => new DoubleRegistrableDisplayItem
                                                                   {
                                                                       Id = rbl.Id,
                                                                       Name = rbl.Name,
                                                                       Tag = rbl.Tag,
                                                                       NameSecondary = rbl.NameSecondary,
                                                                       SpotsAvailable = rbl.MaximumDoubleSeats,
                                                                       HasWaitingList = rbl.HasWaitingList,
                                                                       AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                                                                       MaximumAllowedImbalance = rbl.MaximumAllowedImbalance,
                                                                       LeadersAccepted = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                              && !spt.IsWaitingList
                                                                                                              && spt.RegistrationId != null),
                                                                       FollowersAccepted = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                                && !spt.IsWaitingList
                                                                                                                && spt.RegistrationId_Follower != null),
                                                                       LeadersOnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                                   && spt.IsWaitingList
                                                                                                                   && spt.IsSingleLeaderSpot()),
                                                                       FollowersOnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                                     && spt.IsWaitingList
                                                                                                                     && spt.IsSingleFollowerSpot()),
                                                                       CouplesOnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                                                   && spt.IsWaitingList
                                                                                                                   && (spt.IsUnmatchedPartnerSpot() || spt.IsMatchedPartnerSpot())),
                                                                       IsDeletable = !rbl.Spots!.Any(spt => !spt.IsCancelled)
                                                                                  && userCanDeleteRegistrable
                                                                                  && rbl.Event!.State == RegistrationForms.EventState.Setup,
                                                                       Class = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                                    && !spt.IsWaitingList)
                                                                                  .OrderBy(spt => spt.FirstPartnerJoined)
                                                                                  .Select(GetDoubleSpotState)
                                                                                  .FillUpIf(rbl.MaximumDoubleSeats,
                                                                                            () => new DoubleSpotState { Leader = SpotState.Available, Follower = SpotState.Available })
                                                                                  .ToList(),
                                                                       WaitingList = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                                          && spt.IsWaitingList)
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

public class UpdateRegistrationReadModel : IEventToCommandTranslation<RegistrationProcessed>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateRegistrationReadModel(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public IEnumerable<IRequest> Translate(RegistrationProcessed e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(RegistrablesOverviewQuery),
                             EventId = e.EventId.Value,
                             DirtyMoment = _dateTimeProvider.Now
                         };
        }
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
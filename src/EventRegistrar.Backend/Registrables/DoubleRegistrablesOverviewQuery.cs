using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class DoubleRegistrablesOverviewQuery : IRequest<IEnumerable<DoubleRegistrableDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class DoubleRegistrablesOverviewQueryHandler : IRequestHandler<DoubleRegistrablesOverviewQuery, IEnumerable<DoubleRegistrableDisplayItem>>
{
    private readonly IQueryable<Registrable> _registrables;
    private readonly IAuthorizationChecker _authorizationChecker;

    public DoubleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables,
                                                  IAuthorizationChecker authorizationChecker)
    {
        _registrables = registrables;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<IEnumerable<DoubleRegistrableDisplayItem>> Handle(DoubleRegistrablesOverviewQuery query,
                                                                        CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                         && rbl.MaximumDoubleSeats.HasValue)
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration_Follower)
                                              .ToListAsync(cancellationToken);

        var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(query.EventId, nameof(DeleteRegistrableCommand));
        return registrables.Select(rbl => new DoubleRegistrableDisplayItem
                                          {
                                              Id = rbl.Id,
                                              Name = rbl.Name,
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
                                                         && rbl.Event!.State == RegistrationForms.State.Setup,
                                              Class = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                           && !spt.IsWaitingList)
                                                         .Select(GetSpotState)
                                                         .FillUpIf(rbl.MaximumDoubleSeats, () => new DoubleSpotState { Leader = SpotState.Available, Follower = SpotState.Available })
                                                         .ToList(),
                                              WaitingList = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                 && spt.IsWaitingList)
                                                               .Select(GetSpotState)
                                                               .ToList()
                                          });
    }

    private static DoubleSpotState GetSpotState(Seat spot)
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
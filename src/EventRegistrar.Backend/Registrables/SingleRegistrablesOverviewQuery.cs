using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class SingleRegistrablesOverviewQuery : IRequest<IEnumerable<SingleRegistrableDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class SingleRegistrablesOverviewQueryHandler : IRequestHandler<SingleRegistrablesOverviewQuery, IEnumerable<SingleRegistrableDisplayItem>>
{
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Registration> _registrations;
    private readonly IAuthorizationChecker _authorizationChecker;

    public SingleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables,
                                                  IQueryable<Registration> registrations,
                                                  IAuthorizationChecker authorizationChecker)
    {
        _registrables = registrables;
        _registrations = registrations;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<IEnumerable<SingleRegistrableDisplayItem>> Handle(SingleRegistrablesOverviewQuery query,
                                                                        CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                         && !rbl.MaximumDoubleSeats.HasValue)
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration)
                                              .Include(rbl => rbl.Event)
                                              .ToListAsync(cancellationToken);

        var registrationsOnWaitingList = new HashSet<Guid>(_registrations.Where(reg => reg.EventId == query.EventId
                                                                                    && reg.IsWaitingList == true)
                                                                         .Select(reg => reg.Id));
        var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(query.EventId, nameof(DeleteRegistrableCommand));
        return registrables.OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
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
                                                                              && !registrationsOnWaitingList.Contains(spt.RegistrationId ??
                                                                                                                      Guid.Empty)),
                                              OnWaitingList = rbl.Spots!.Count(spt => !spt.IsCancelled
                                                                                   && (spt.IsWaitingList ||
                                                                                       registrationsOnWaitingList.Contains(spt.RegistrationId ??
                                                                                                                           Guid.Empty))),
                                              IsDeletable = !rbl.Spots!.Any(spt => !spt.IsCancelled)
                                                         && userCanDeleteRegistrable
                                                         && rbl.Event!.State == RegistrationForms.State.Setup,
                                              Class = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                           && !spt.IsWaitingList)
                                                         .Select(GetSpotState)
                                                         .FillUpIf(rbl.MaximumSingleSeats, () => SpotState.Available)
                                                         .ToList(),
                                              WaitingList = rbl.Spots!.Where(spt => !spt.IsCancelled
                                                                                 && spt.IsWaitingList)
                                                               .Select(GetSpotState)
                                                               .ToList()
                                          });
    }

    private static SpotState GetSpotState(Seat spot)
    {
        return spot.Registration == null
            ? SpotState.Available
            : spot.Registration.State == RegistrationState.Paid
                ? SpotState.Paid
                : SpotState.Registered;
    }
}
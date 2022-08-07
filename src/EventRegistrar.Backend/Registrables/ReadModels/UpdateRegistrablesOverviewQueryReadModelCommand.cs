using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

using MediatR;

namespace EventRegistrar.Backend.Registrables.ReadModels;

public class UpdateRegistrablesOverviewQueryReadModelCommand : IRequest
{
    public Guid EventId { get; set; }
}

public class UpdateRegistrablesOverviewQueryReadModelCommandHandler : IRequestHandler<UpdateRegistrablesOverviewQueryReadModelCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IEventBus _eventBus;

    private readonly DbContext _dbContext;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IAuthorizationChecker _authorizationChecker;

    public UpdateRegistrablesOverviewQueryReadModelCommandHandler(IQueryable<Registration> registrations,
                                                                  DbContext dbContext,
                                                                  IEventBus eventBus,
                                                                  IQueryable<Registrable> registrables,
                                                                  IAuthorizationChecker authorizationChecker)
    {
        _registrations = registrations;
        _dbContext = dbContext;
        _eventBus = eventBus;
        _registrables = registrables;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Unit> Handle(UpdateRegistrablesOverviewQueryReadModelCommand command, CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == command.EventId)
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration)
                                              .Include(rbl => rbl.Spots!)
                                              .ThenInclude(spt => spt.Registration_Follower)
                                              .Include(rbl => rbl.Event)
                                              .ToListAsync(cancellationToken);

        var registrationsOnWaitingList = new HashSet<Guid>(_registrations.Where(reg => reg.EventId == command.EventId
                                                                                    && reg.IsWaitingList == true)
                                                                         .Select(reg => reg.Id));

        var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(command.EventId, nameof(DeleteRegistrableCommand));

        var content = new RegistrablesOverview
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
                                                                                         && rbl.Event!.State == RegistrationForms.State.Setup,
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
        var readModels = _dbContext.Set<RegistrablesOverviewQueryReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rm => rm.EventId == command.EventId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel == null)
        {
            readModel = new RegistrablesOverviewQueryReadModel
                        {
                            EventId = command.EventId,
                            Content = content
                        };
            var entry = readModels.Attach(readModel);
            entry.State = EntityState.Added;
            _eventBus.Publish(new ReadModelUpdated
                              {
                                  EventId = command.EventId,
                                  QueryName = nameof(RegistrablesOverviewQueryReadModel)
                              });
        }
        else
        {
            readModel.Content = content;
            if (_dbContext.Entry(readModel).State == EntityState.Modified)
            {
                _eventBus.Publish(new ReadModelUpdated
                                  {
                                      EventId = command.EventId,
                                      QueryName = nameof(RegistrablesOverviewQueryReadModel)
                                  });
            }
        }

        return Unit.Value;
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

//public class UpdateRegistrationWhenOutgoingPaymentAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>
//{
//    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
//    {
//        if (e.EventId != null && e.RegistrationId != null)
//        {
//            yield return new UpdateRegistrationQueryReadModelCommand
//                         {
//                             EventId = e.EventId.Value,
//                             RegistrationId = e.RegistrationId.Value
//                         };
//        }
//    }
//}
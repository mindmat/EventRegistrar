using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots;

public class SpotsOfRegistrationQuery : IRequest<IEnumerable<SpotDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SpotsOfRegistrationQueryHandler(IQueryable<Seat> seats,
                                             EnumTranslator enumTranslator)
    : IRequestHandler<SpotsOfRegistrationQuery, IEnumerable<SpotDisplayItem>>
{
    public async Task<IEnumerable<SpotDisplayItem>> Handle(SpotsOfRegistrationQuery query,
                                                           CancellationToken cancellationToken)
    {
        var spots = await seats.Where(spot => (spot.Registration!.EventId == query.EventId
                                            || spot.Registration_Follower!.EventId == query.EventId)
                                           && (spot.RegistrationId == query.RegistrationId
                                            || spot.RegistrationId_Follower == query.RegistrationId))
                               .Where(spot => !spot.IsCancelled)
                               .OrderByDescending(spot => spot.Registrable!.IsCore)
                               .ThenBy(spot => spot.Registrable!.ShowInMailListOrder)
                               .Select(spot => new SpotDisplayItem
                                               {
                                                   Id = spot.Id,
                                                   RegistrableId = spot.RegistrableId,
                                                   RegistrableName = spot.Registrable!.Name,
                                                   RegistrableNameSecondary = spot.Registrable.NameSecondary,
                                                   PartnerRegistrationId = spot.IsPartnerSpot
                                                                               ? spot.RegistrationId == query.RegistrationId
                                                                                     ? spot.RegistrationId_Follower
                                                                                     : spot.RegistrationId
                                                                               : null,
                                                   FirstPartnerJoined = spot.FirstPartnerJoined,
                                                   IsCore = spot.Registrable.IsCore,
                                                   RoleText = spot.Registrable.Type == RegistrableType.Double
                                                                  ? enumTranslator.Translate(spot.RegistrationId == query.RegistrationId
                                                                                                 ? Role.Leader
                                                                                                 : Role.Follower)
                                                                  : null,
                                                   PartnerName = spot.IsPartnerSpot
                                                                     ? spot.RegistrationId == query.RegistrationId
                                                                           ? $"{spot.Registration_Follower!.RespondentFirstName} {spot.Registration_Follower.RespondentLastName}"
                                                                           : $"{spot.Registration!.RespondentFirstName} {spot.Registration.RespondentLastName}"
                                                                     : null,
                                                   IsWaitingList = spot.IsWaitingList,
                                                   Type = spot.Registrable.Type
                                               })
                               .ToListAsync(cancellationToken);

        return spots;
    }
}
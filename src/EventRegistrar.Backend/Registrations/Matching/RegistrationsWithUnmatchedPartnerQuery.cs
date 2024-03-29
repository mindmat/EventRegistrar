﻿namespace EventRegistrar.Backend.Registrations.Matching;

public class RegistrationsWithUnmatchedPartnerQuery : IEventBoundRequest, IRequest<IEnumerable<PotentialPartnerMatch>>
{
    public Guid EventId { get; set; }
}

public class RegistrationsWithUnmatchedPartnerQueryHandler(IQueryable<Registration> registrations) : IRequestHandler<RegistrationsWithUnmatchedPartnerQuery, IEnumerable<PotentialPartnerMatch>>
{
    public async Task<IEnumerable<PotentialPartnerMatch>> Handle(RegistrationsWithUnmatchedPartnerQuery query,
                                                                 CancellationToken cancellationToken)
    {
        return await registrations.Where(reg => reg.EventId == query.EventId
                                             && reg.State != RegistrationState.Cancelled
                                             && reg.PartnerNormalized != null
                                             && reg.RegistrationId_Partner == null)
                                  .OrderByDescending(reg => reg.ReceivedAt)
                                  .Select(reg => new PotentialPartnerMatch
                                                 {
                                                     RegistrationId = reg.Id,
                                                     Email = reg.RespondentEmail,
                                                     FirstName = reg.RespondentFirstName,
                                                     LastName = reg.RespondentLastName,
                                                     State = reg.State.ToString(),
                                                     DeclaredPartner = reg.PartnerOriginal,
                                                     IsOnWaitingList = reg.IsOnWaitingList == true,
                                                     Tracks = reg.Seats_AsLeader!
                                                                 .Select(spt => new TrackMatch { Name = spt.Registrable!.DisplayName })
                                                                 .Union(reg.Seats_AsFollower!.Select(spt => new TrackMatch { Name = spt.Registrable!.DisplayName }))
                                                                 .ToArray()
                                                 })
                                  .ToListAsync(cancellationToken);
    }
}
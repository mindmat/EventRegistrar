using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Registrations.Matching;

public class PotentialPartnersQuery : IEventBoundRequest, IRequest<PotentialPartners>
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? SearchString { get; set; }
}

public class PotentialPartnersQueryHandler : IRequestHandler<PotentialPartnersQuery, PotentialPartners>
{
    private readonly IQueryable<Registration> _registrations;

    public PotentialPartnersQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<PotentialPartners> Handle(PotentialPartnersQuery query,
                                                CancellationToken cancellationToken)
    {
        var ownRegistration = await _registrations.Where(reg => reg.EventId == query.EventId
                                                             && reg.Id == query.RegistrationId)
                                                  .Select(reg => new
                                                                 {
                                                                     reg.Id,
                                                                     reg.RespondentFirstName,
                                                                     reg.RespondentLastName,
                                                                     reg.RespondentEmail,
                                                                     reg.State,
                                                                     reg.PartnerNormalized,
                                                                     reg.PartnerOriginal,
                                                                     IsOnWaitingList = reg.IsOnWaitingList == true,
                                                                     PartnerRegistrableAsLeader = reg.Seats_AsLeader!.Where(spt => spt.Registrable!.MaximumDoubleSeats != null)
                                                                                                     .Select(trk => trk.Registrable!),
                                                                     PartnerRegistrableAsFollower = reg.Seats_AsFollower!.Where(spt => spt.Registrable!.MaximumDoubleSeats != null)
                                                                                                       .Select(trk => trk.Registrable!)
                                                                 })
                                                  .FirstAsync(cancellationToken);

        var ownPartnerTracks = ownRegistration.PartnerRegistrableAsLeader
                                              .Union(ownRegistration.PartnerRegistrableAsFollower)
                                              .ToList();
        var ownPartnerTrackIds = ownPartnerTracks.Select(trk => trk.Id)
                                                 .ToList();

        var searchParts = (query.SearchString ?? ownRegistration.PartnerNormalized)?.Split(" ");
        if (searchParts == null || searchParts.Length == 0)
        {
            throw new ArgumentException("No search string");
        }

        var partnerRegistrableId = ownRegistration.PartnerRegistrableAsLeader.FirstOrDefault()?.Id
                                ?? ownRegistration.PartnerRegistrableAsFollower.FirstOrDefault()?.Id;
        if (partnerRegistrableId == null)
        {
            throw new ArgumentException("No partner spot found");
        }

        var otherRole = ownRegistration.PartnerRegistrableAsLeader.FirstOrDefault() != null
                            ? Role.Follower
                            : Role.Leader;

        var queryable = _registrations.Where(reg => reg.EventId == query.EventId)
                                      .WhereIf(otherRole == Role.Leader, reg => reg.Seats_AsLeader!.Any(spt => !spt.IsCancelled && spt.RegistrableId == partnerRegistrableId))
                                      .WhereIf(otherRole == Role.Follower, reg => reg.Seats_AsFollower!.Any(spt => !spt.IsCancelled && spt.RegistrableId == partnerRegistrableId))
                                      .Select(reg => new
                                                     {
                                                         RegistrationId = reg.Id,
                                                         Email = reg.RespondentEmail,
                                                         FirstName = reg.RespondentFirstName,
                                                         LastName = reg.RespondentLastName,
                                                         State = reg.State.ToString(),
                                                         Partner = reg.PartnerOriginal,
                                                         IsWaitingList = reg.IsOnWaitingList,
                                                         reg.RegistrationId_Partner,
                                                         MatchedPartnerFirstName = reg.Registration_Partner!.RespondentFirstName,
                                                         MatchedPartnerLastName = reg.Registration_Partner.RespondentLastName,
                                                         Registrables = reg.Seats_AsLeader!.Where(spt => spt.Registrable!.MaximumDoubleSeats != null)
                                                                           .Select(spt => spt.Registrable!)
                                                                           .Union(reg.Seats_AsFollower!.Where(spt => spt.Registrable!.MaximumDoubleSeats != null).Select(spt => spt.Registrable!))
                                                     });
        foreach (var searchPart in searchParts)
        {
            queryable = queryable.Where(mat => EF.Functions.Like(mat.Email!, $"%{searchPart}%")
                                            || EF.Functions.Like(mat.FirstName!, $"%{searchPart}%")
                                            || EF.Functions.Like(mat.LastName!, $"%{searchPart}%"));
        }

        var matches = await queryable.Select(mat => new PotentialPartnerMatch
                                                    {
                                                        RegistrationId = mat.RegistrationId,
                                                        Email = mat.Email,
                                                        FirstName = mat.FirstName,
                                                        LastName = mat.LastName,
                                                        State = mat.State,
                                                        DeclaredPartner = mat.Partner,
                                                        Tracks = mat.Registrables.Select(trk => new TrackMatch
                                                                                                {
                                                                                                    Name = trk.DisplayName,
                                                                                                    Match = ownPartnerTrackIds.Contains(trk.Id)
                                                                                                                ? TracksMatch.Some
                                                                                                                : TracksMatch.None
                                                                                                }),
                                                        IsOnWaitingList = mat.IsWaitingList == true,
                                                        MatchedPartner = $"{mat.MatchedPartnerFirstName} {mat.MatchedPartnerLastName}",
                                                        RegistrationId_Partner = mat.RegistrationId_Partner
                                                    })
                                     .ToListAsync(cancellationToken);

        foreach (var match in matches)
        {
            if (match.Tracks!.All(trk => trk.Match == TracksMatch.Some))
            {
                match.Tracks!.ForEach(trk => trk.Match = TracksMatch.All);
            }
        }

        return new PotentialPartners
               {
                   RegistrationId = ownRegistration.Id,
                   Name = $"{ownRegistration.RespondentFirstName} {ownRegistration.RespondentLastName}",
                   Email = ownRegistration.RespondentEmail,
                   IsOnWaitingList = ownRegistration.IsOnWaitingList,
                   DeclaredPartner = ownRegistration.PartnerOriginal,
                   State = ownRegistration.State,
                   Tracks = ownPartnerTracks.Select(trk => trk.DisplayName).ToList(),
                   Matches = matches
               };
    }
}

public class PotentialPartners
{
    public Guid RegistrationId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool IsOnWaitingList { get; set; }
    public RegistrationState State { get; set; }
    public string? DeclaredPartner { get; set; }
    public IEnumerable<string>? Tracks { get; set; }
    public IEnumerable<PotentialPartnerMatch>? Matches { get; set; }
}
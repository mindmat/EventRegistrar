using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class SpotMatchCandidatesQuery : IRequest<SpotMatchCandidates>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public Role Role { get; set; }
    public string? SearchString { get; set; }
}

public class SpotMatchCandidatesQueryHandler(IQueryable<Seat> spots) : IRequestHandler<SpotMatchCandidatesQuery, SpotMatchCandidates>
{
    public async Task<SpotMatchCandidates> Handle(SpotMatchCandidatesQuery query, CancellationToken cancellationToken)
    {
        var queryable = spots.Where(spt => spt.RegistrableId == query.RegistrableId
                                        && !spt.IsCancelled
                                        && !spt.IsPartnerSpot)
                             .WhereIf(query.Role == Role.Leader, spt => spt.RegistrationId != null
                                                                     && spt.RegistrationId_Follower == null)
                             .WhereIf(query.Role == Role.Follower, spt => spt.RegistrationId == null
                                                                       && spt.RegistrationId_Follower != null);

        var searchParts = query.SearchString?.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (searchParts?.Length > 0)
        {
            foreach (var searchPart in searchParts)
            {
                if (query.Role == Role.Leader)
                {
                    queryable = queryable.Where(mat => EF.Functions.Like(mat.Registration!.RespondentEmail!, $"%{searchPart}%")
                                                    || EF.Functions.Like(mat.Registration!.RespondentFirstName!, $"%{searchPart}%")
                                                    || EF.Functions.Like(mat.Registration!.RespondentLastName!, $"%{searchPart}%"));
                }
                else
                {
                    queryable = queryable.Where(mat => EF.Functions.Like(mat.Registration_Follower!.RespondentEmail!, $"%{searchPart}%")
                                                    || EF.Functions.Like(mat.Registration_Follower!.RespondentFirstName!, $"%{searchPart}%")
                                                    || EF.Functions.Like(mat.Registration_Follower!.RespondentLastName!, $"%{searchPart}%"));
                }
            }
        }

        List<SpotMatchCandidate> candidates;
        if (query.Role == Role.Leader)
        {
            candidates = await queryable.Select(spt => new SpotMatchCandidate
                                                       {
                                                           SpotId = spt.Id,
                                                           RegistrationId = spt.RegistrationId!.Value,
                                                           Name =  $"{spt.Registration!.RespondentFirstName} {spt.Registration!.RespondentLastName}"
                                                       })
                                        .ToListAsync(cancellationToken);
        }
        else
        {
            candidates = await queryable.Select(spt => new SpotMatchCandidate
                                                       {
                                                           SpotId = spt.Id,
                                                           RegistrationId = spt.RegistrationId_Follower!.Value,
                                                           Name = $"{spt.Registration_Follower!.RespondentFirstName} {spt.Registration_Follower!.RespondentLastName}"
                                                       })
                                        .ToListAsync(cancellationToken);
        }

        return new SpotMatchCandidates
               {
                   RegistrableId = query.RegistrableId,
                   Role = query.Role,
                   Candidates = candidates
               };
    }
}

public class SpotMatchCandidates
{
    public Guid RegistrableId { get; set; }
    public Role Role { get; set; }
    public IEnumerable<SpotMatchCandidate> Candidates { get; set; } = null!;
}

public class SpotMatchCandidate
{
    public Guid SpotId { get; set; }
    public string Name { get; set; } = null!;
    public Guid RegistrationId { get; set; }
}
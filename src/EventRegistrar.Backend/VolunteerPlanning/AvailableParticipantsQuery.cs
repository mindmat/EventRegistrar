using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class AvailableParticipantsQuery : IRequest<IEnumerable<ParticipantDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
    public string? SearchString { get; set; }
}

public class ParticipantDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsAlreadyAssigned { get; set; }
    public bool PrefersTime { get; set; }
}

public class AvailableParticipantsQueryHandler(IQueryable<Registration> registrations,
                                               IQueryable<Shift> shifts,
                                               IQueryable<ShiftAssignment> assignments,
                                               VolunteerAdminConfiguration config)
    : IRequestHandler<AvailableParticipantsQuery, IEnumerable<ParticipantDisplayItem>>
{
    public async Task<IEnumerable<ParticipantDisplayItem>> Handle(AvailableParticipantsQuery query, CancellationToken cancellationToken)
    {
        var shiftData = await shifts.Where(sft => sft.Id == query.ShiftId)
                                    .Select(sft => new
                                                   {
                                                       sft.RegistrationId_Responsible,
                                                       Helpers = sft.Assignments!.Select(sas => sas.RegistrationId),
                                                       sft.RegistrableId_ShiftPreference
                                                   })
                                    .FirstAsync(cancellationToken);
        var registrationIds_AlreadyAssigned = new HashSet<Guid>();
        if (shiftData.RegistrationId_Responsible != null)
        {
            registrationIds_AlreadyAssigned.Add(shiftData.RegistrationId_Responsible.Value);
        }

        registrationIds_AlreadyAssigned.AddRange(shiftData.Helpers);

        // Get all registrations for the event that are admitted
        var queryable = registrations.Where(reg => reg.EventId == query.EventId
                                                && reg.State != RegistrationState.Cancelled
                                                && reg.IsOnWaitingList == false)
                                     .WhereIf(config.RegistrableIds_Volunteer.HasElements(),
                                              reg => reg.Seats_AsLeader!.Any(spot => config.RegistrableIds_Volunteer!.Contains(spot.RegistrableId))
                                                  || reg.Seats_AsFollower!.Any(spot => config.RegistrableIds_Volunteer!.Contains(spot.RegistrableId)));

        var searchParts = query.SearchString?.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       ?? [];

        foreach (var searchPart in searchParts)
        {
            queryable = queryable.Where(mat => EF.Functions.Like(mat.RespondentFirstName!, $"%{searchPart}%")
                                            || EF.Functions.Like(mat.RespondentLastName!, $"%{searchPart}%"));
        }

        var candidates = await queryable.Select(reg => new ParticipantDisplayItem
                                                       {
                                                           RegistrationId = reg.Id,
                                                           FirstName = reg.RespondentFirstName,
                                                           LastName = reg.RespondentLastName,
                                                           Email = reg.RespondentEmail,
                                                           IsAlreadyAssigned = registrationIds_AlreadyAssigned.Contains(reg.Id),
                                                           PrefersTime = reg.Seats_AsLeader!.Any(spt => !spt.IsCancelled
                                                                                                     && spt.RegistrableId == shiftData.RegistrableId_ShiftPreference)
                                                       })
                                        .ToListAsync(cancellationToken);


        return candidates.OrderBy(reg => reg.IsAlreadyAssigned)
                         .ThenByDescending(reg => reg.PrefersTime)
                         .ThenBy(reg => reg.LastName)
                         .ThenBy(reg => reg.FirstName);
    }
}
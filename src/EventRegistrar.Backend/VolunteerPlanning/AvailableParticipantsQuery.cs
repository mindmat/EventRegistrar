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
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsAlreadyAssigned { get; set; }
    public string? PreferredTimes { get; set; }
}

public class AvailableParticipantsQueryHandler(IQueryable<Registration> registrations,
                                               IQueryable<Shift> shifts,
                                               IQueryable<ShiftAssignment> assignments,
                                               VolunteerAdminConfiguration config)
    : IRequestHandler<AvailableParticipantsQuery, IEnumerable<ParticipantDisplayItem>>
{
    public async Task<IEnumerable<ParticipantDisplayItem>> Handle(AvailableParticipantsQuery query, CancellationToken cancellationToken)
    {
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

        var registrationData = await queryable.Include(r => r.Responses!)
                                              .ThenInclude(resp => resp.Question)
                                              .ToListAsync(cancellationToken);

        // Get existing assignments for the shift
        var assignmentData = await assignments.Where(sas => sas.ShiftId == query.ShiftId)
                                              .Select(sas => sas.RegistrationId)
                                              .ToListAsync(cancellationToken);

        var assignedRegistrationIds = assignmentData.ToHashSet();

        var responsibleRegistrationId = await shifts.Where(sft => sft.Id == query.ShiftId)
                                                    .Select(sft => sft.RegistrationId_Responsible)
                                                    .FirstOrDefaultAsync(cancellationToken);

        var responsibleRegistrationIds = new HashSet<Guid>();
        if (responsibleRegistrationId.HasValue)
        {
            responsibleRegistrationIds.Add(responsibleRegistrationId.Value);
        }

        var participants = registrationData.Select(registration =>
        {
            // Look for volunteer time preferences in responses
            var volunteerTimeResponse = registration.Responses?
                .FirstOrDefault(r => r.Question != null
                                  && r.Question.Title != null
                                  && (r.Question.Title.Contains("volunteer", StringComparison.OrdinalIgnoreCase)
                                   || r.Question.Title.Contains("helper", StringComparison.OrdinalIgnoreCase)
                                   || r.Question.Title.Contains("time", StringComparison.OrdinalIgnoreCase)));

            return new ParticipantDisplayItem
                   {
                       RegistrationId = registration.Id,
                       FirstName = registration.RespondentFirstName ?? "",
                       LastName = registration.RespondentLastName ?? "",
                       Email = registration.RespondentEmail,
                       IsAlreadyAssigned = assignedRegistrationIds.Contains(registration.Id) || responsibleRegistrationIds.Contains(registration.Id),
                       PreferredTimes = volunteerTimeResponse?.ResponseString
                   };
        });

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.SearchString))
        {
            var searchTerm = query.SearchString.Trim();
            participants = participants.Where(p =>
                                                  (!string.IsNullOrEmpty(p.FirstName) && p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                                               || (!string.IsNullOrEmpty(p.LastName) && p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                                               || (!string.IsNullOrEmpty(p.Email) && p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        return participants.OrderBy(p => p.LastName)
                           .ThenBy(p => p.FirstName);
    }
}
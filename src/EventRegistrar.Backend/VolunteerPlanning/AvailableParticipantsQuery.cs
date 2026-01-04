using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class AvailableParticipantsQuery : IRequest<IEnumerable<ParticipantDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid? ShiftId { get; set; }
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
                                               IQueryable<ShiftAssignment> assignments)
    : IRequestHandler<AvailableParticipantsQuery, IEnumerable<ParticipantDisplayItem>>
{
    public async Task<IEnumerable<ParticipantDisplayItem>> Handle(AvailableParticipantsQuery query, CancellationToken cancellationToken)
    {
        // Get all registrations for the event that are admitted
        var registrationData = await registrations.Where(reg => reg.EventId == query.EventId
                                                             && reg.State != RegistrationState.Cancelled
                                                             && reg.IsOnWaitingList == false)
                                                  .Include(r => r.Responses!)
                                                  .ThenInclude(resp => resp.Question)
                                                  .ToListAsync(cancellationToken);

        // Get existing assignments for the specific shift (if provided)
        HashSet<Guid> assignedRegistrationIds = [];
        HashSet<Guid> responsibleRegistrationIds = [];

        if (query.ShiftId != null)
        {
            var assignmentData = await assignments.Where(sas => sas.ShiftId == query.ShiftId.Value)
                                                  .Select(sas => sas.RegistrationId)
                                                  .ToListAsync(cancellationToken);

            assignedRegistrationIds = assignmentData.ToHashSet();

            var responsibleRegistrationId = await shifts.Where(sft => sft.Id == query.ShiftId.Value)
                                                        .Select(sft => sft.RegistrationId_Responsible)
                                                        .FirstOrDefaultAsync(cancellationToken);

            if (responsibleRegistrationId.HasValue)
            {
                responsibleRegistrationIds.Add(responsibleRegistrationId.Value);
            }
        }

        return registrationData.Select(registration =>
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
                               })
                               .OrderBy(p => p.LastName)
                               .ThenBy(p => p.FirstName);
    }
}
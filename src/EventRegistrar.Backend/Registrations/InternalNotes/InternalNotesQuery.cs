namespace EventRegistrar.Backend.Registrations.InternalNotes;

public class InternalNotesQuery : IRequest<IEnumerable<NotesDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class InternalNotesQueryHandler(IQueryable<Registration> registrations) : IRequestHandler<InternalNotesQuery, IEnumerable<NotesDisplayItem>>
{
    public async Task<IEnumerable<NotesDisplayItem>> Handle(InternalNotesQuery query, CancellationToken cancellationToken)
    {
        return await registrations.Where(reg => reg.EventId == query.EventId
                                             && reg.InternalNotes != null
                                             && reg.InternalNotes != string.Empty)
                                  .OrderByDescending(reg => reg.ReceivedAt)
                                  .Select(reg => new NotesDisplayItem
                                                 {
                                                     RegistrationId = reg.Id,
                                                     DisplayName = $"{reg.RespondentFirstName} {reg.RespondentLastName}",
                                                     Email = reg.RespondentEmail,
                                                     Notes = reg.InternalNotes!
                                                 })
                                  .ToListAsync(cancellationToken);
    }
}

public class NotesDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string Notes { get; set; } = null!;
}
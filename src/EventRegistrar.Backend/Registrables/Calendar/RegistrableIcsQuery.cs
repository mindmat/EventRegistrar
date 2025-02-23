namespace EventRegistrar.Backend.Registrables.Calendar;

public record RegistrableIcsItem(Guid Id,
                                 Guid RegistrableId,
                                 bool AddToCalendar,
                                 string? Title,
                                 string? Location,
                                 DateTimeOffset Start,
                                 DateTimeOffset End,
                                 string? ContentHtml);

public class RegistrableIcsQuery : IEventBoundRequest, IRequest<RegistrableIcsItem>
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrableIcsId { get; set; }
}

public class RegistrableIcsQueryHandler(IQueryable<RegistrableIcs> registrablesIcs) : IRequestHandler<RegistrableIcsQuery, RegistrableIcsItem>
{
    public async Task<RegistrableIcsItem> Handle(RegistrableIcsQuery query,
                                                 CancellationToken cancellationToken)
    {
        return await registrablesIcs.Where(rbl => rbl.Registrable!.EventId == query.EventId
                                               && rbl.RegistrableId == query.RegistrableId
                                               && rbl.Id == query.RegistrableIcsId)
                                    .Select(rbl => new RegistrableIcsItem
                                            (
                                                rbl.Id,
                                                rbl.RegistrableId,
                                                rbl.AddToCalendar,
                                                rbl.Title,
                                                rbl.Location,
                                                rbl.Start,
                                                rbl.End,
                                                rbl.ContentHtml
                                            ))
                                    .FirstAsync(cancellationToken);
    }
}
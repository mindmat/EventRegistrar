namespace EventRegistrar.Backend.Registrables.Calendar;

public record CalendarIcsItem(Guid Id,
                              Guid RegistrableId,
                              string? RegistrableName,
                              string? RegistrableNameSecondary,
                              string? Title,
                              string? Location,
                              DateTimeOffset Start,
                              DateTimeOffset End,
                              string? ContentHtml);

public record LocationGroup(string? Location,
                            IEnumerable<CalendarIcsItem> Items);

public record DateGroup(DateTime Date,
                        IEnumerable<LocationGroup> LocationGroups);

public class TracksCalendarQuery : IEventBoundRequest, IRequest<IEnumerable<DateGroup>>
{
    public Guid EventId { get; set; }
}

public class TracksCalendarQueryHandler(IQueryable<RegistrableIcs> registrablesIcs)
    : IRequestHandler<TracksCalendarQuery, IEnumerable<DateGroup>>
{
    public async Task<IEnumerable<DateGroup>> Handle(TracksCalendarQuery query,
                                                     CancellationToken cancellationToken)
    {
        var icsItems = await registrablesIcs.Where(ics => ics.Registrable!.EventId == query.EventId
                                                       && ics.AddToCalendar)
                                            .OrderBy(ics => ics.Start)
                                            .Select(ics => new CalendarIcsItem(ics.Id,
                                                                               ics.RegistrableId,
                                                                               ics.Registrable!.Name,
                                                                               ics.Registrable!.NameSecondary,
                                                                               ics.Title,
                                                                               ics.Location,
                                                                               ics.Start,
                                                                               ics.End,
                                                                               ics.ContentHtml))
                                            .ToListAsync(cancellationToken);

        var locationGroups = icsItems.GroupBy(ics => ics.Start.Date)
                                     .Select(dateGroup => new DateGroup(dateGroup.Key,
                                                                        dateGroup.GroupBy(ics => ics.Location ?? "?")
                                                                                 .Select(locationGroup => new LocationGroup(
                                                                                             locationGroup.Key,
                                                                                             locationGroup.OrderBy(ics => ics.Start).ToList()))
                                                                                 .OrderBy(lg => lg.Location)
                                                                                 .ToList()))
                                     .OrderBy(dg => dg.Date)
                                     .ToList();

        return locationGroups;
    }
}
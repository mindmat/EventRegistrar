using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrables.Calendar;

public class SaveRegistrableIcsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public RegistrableIcsItem? RegistrableIcsItem { get; set; }
}

public class SaveRegistrableIcsCommandHandler(IRepository<RegistrableIcs> registrablesIcs,
                                              ChangeTrigger changeTrigger,
                                              CalendarConfiguration configuration) : IRequestHandler<SaveRegistrableIcsCommand>
{
    public async Task Handle(SaveRegistrableIcsCommand command, CancellationToken cancellationToken)
    {
        if (command.RegistrableIcsItem == null)
        {
            throw new ArgumentNullException(nameof(SaveRegistrableIcsCommand.RegistrableIcsItem));
        }

        var saveItem = command.RegistrableIcsItem;
        var ics = await registrablesIcs.AsTracking()
                                       .Where(rbl => rbl.Id == saveItem.Id
                                                  && rbl.RegistrableId == saveItem.RegistrableId
                                                  && rbl.Registrable!.EventId == command.EventId)
                                       .FirstOrDefaultAsync(cancellationToken)
               ?? registrablesIcs.InsertObjectTree(new RegistrableIcs
                                                   {
                                                       Id = saveItem.Id,
                                                       RegistrableId = saveItem.RegistrableId
                                                   });

        ics.AddToCalendar = saveItem.AddToCalendar;
        ics.Location = saveItem.Location;
        ics.Start = ConvertUtcToEventTime(saveItem.Start);
        ics.End = ConvertUtcToEventTime(saveItem.End);
        ics.ContentHtml = saveItem.ContentHtml;

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
    }

    private DateTime ConvertUtcToEventTime(DateTimeOffset dateTime)
    {
        try
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, configuration.TimeZone).LocalDateTime;
        }
        catch
        {
            if (configuration.FallbackOffset != null)
            {
                return dateTime.Add(configuration.FallbackOffset.Value).LocalDateTime;
            }

            return dateTime.LocalDateTime;
        }
    }
}
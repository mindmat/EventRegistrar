using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Calendar;

namespace EventRegistrar.Backend.Registrables.Calendar
{
    public class SaveRegistrableIcsCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public RegistrableIcsItem? RegistrableIcsItem { get; set; }
    }
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

        var ics = await registrablesIcs.AsTracking()
                                       .FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableIcsItem.RegistrableId,
                                                            cancellationToken)
               ?? registrablesIcs.InsertObjectTree(new RegistrableIcs { Id = command.RegistrableIcsItem.RegistrableId });


        ics.AddToCalendar = command.RegistrableIcsItem.AddToCalendar;
        ics.Location = command.RegistrableIcsItem.Location;
        ics.Start = ConvertUtcToEventTime(command.RegistrableIcsItem.Start);
        ics.End = ConvertUtcToEventTime(command.RegistrableIcsItem.End);
        ics.ContentHtml = command.RegistrableIcsItem.ContentHtml;

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
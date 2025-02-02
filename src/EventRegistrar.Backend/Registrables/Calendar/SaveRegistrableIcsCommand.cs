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
                                              ChangeTrigger changeTrigger) : IRequestHandler<SaveRegistrableIcsCommand>
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
        ics.Start = command.RegistrableIcsItem.Start;
        ics.End = command.RegistrableIcsItem.End;
        ics.ContentHtml = command.RegistrableIcsItem.ContentHtml;

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
    }
}
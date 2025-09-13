using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.MenuNodes;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RegistrationFormsMenuNodeCalculation(IQueryable<Event> events,
                                                  IQueryable<RawRegistrationForm> rawForms)
    : IMenuNodeCalculator
{
    public MenuNodeKey Key => MenuNodeKey.Forms;

    public async Task<MenuNodeCalculation> Calculate(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == eventId, cancellationToken);
        var pendingRawFormCount = await rawForms.CountAsync(frm => frm.EventAcronym == @event.Acronym
                                               && frm.Processed == null,cancellationToken);

        return pendingRawFormCount switch
        {
            > 0 => new MenuNodeCalculation
                   {
                       Key = Key,
                       Content = pendingRawFormCount.ToString(),
                       Style = MenuNodeStyle.ToDo,
                       Hidden = false
                   },
            _ => new MenuNodeCalculation
                 {
                     Key = Key,
                     Content = null,
                     Style = null,
                     Hidden = false
                 }
        };
    }
}
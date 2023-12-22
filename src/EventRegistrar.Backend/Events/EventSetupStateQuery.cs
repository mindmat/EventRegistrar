using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;

namespace EventRegistrar.Backend.Events;

public class EventSetupStateQuery : IRequest<EventSetupState>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class EventSetupState
{
    public bool FormSent { get; set; }
    public bool FormImported { get; set; }
    public bool TracksDefined { get; set; }
    public bool FormMapped { get; set; }
}

public class EventSetupStateQueryHandler(IQueryable<Event> events,
                                         IQueryable<RawRegistrationForm> rawRegistrationForms,
                                         IQueryable<RegistrationForm> registrationForms)
    : IRequestHandler<EventSetupStateQuery, EventSetupState>
{
    public async Task<EventSetupState> Handle(EventSetupStateQuery query, CancellationToken cancellationToken)
    {
        var @event = await events.Where(evt => evt.Id == query.EventId)
                                 .Select(evt => new
                                                {
                                                    evt.Acronym,
                                                    AnyTracks = evt.Registrables!.Any()
                                                })
                                 .FirstAsync(cancellationToken);
        var formSent = await rawRegistrationForms.AnyAsync(rrf => rrf.EventAcronym == @event.Acronym, cancellationToken);
        var form = await registrationForms.Where(frm => frm.EventId == query.EventId)
                                          .Select(frm => new
                                                         {
                                                             frm.Id,
                                                             Mapped = frm.Questions!.Any(qst => qst.Mapping != null
                                                                                             || qst.QuestionOptions!.Any(qop => qop.Mappings!.Any()))
                                                                   || frm.MultiMappings!.Any()
                                                         })
                                          .FirstOrDefaultAsync(cancellationToken);

        return new EventSetupState
               {
                   FormSent = formSent,
                   FormImported = form != null,
                   TracksDefined = @event.AnyTracks,
                   FormMapped = form?.Mapped == true
               };
    }
}
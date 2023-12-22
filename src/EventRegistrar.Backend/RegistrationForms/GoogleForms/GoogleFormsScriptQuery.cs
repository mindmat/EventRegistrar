using EventRegistrar.Backend.Events;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class GoogleFormsScriptQuery : IRequest<string>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class GoogleFormsScriptQueryHandler(IQueryable<Event> events)
    : IRequestHandler<GoogleFormsScriptQuery, string>
{
    public async Task<string> Handle(GoogleFormsScriptQuery query, CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == query.EventId, cancellationToken);
        return Properties.Resources.GoogleFormsScript.Replace("{eventAcronym}", @event.Acronym);
    }
}
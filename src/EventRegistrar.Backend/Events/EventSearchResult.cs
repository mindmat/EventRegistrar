using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events;

public class EventSearchResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Acronym { get; set; } = null!;
    public bool RequestSent { get; set; }
    public EventState State { get; set; }
    public string StateText { get; set; } = null!;
}
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events;

public class EventSearchResult
{
    public string Acronym { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool RequestSent { get; set; }
    public State State { get; set; }
}
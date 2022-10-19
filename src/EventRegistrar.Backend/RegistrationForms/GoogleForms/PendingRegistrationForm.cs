using EventRegistrar.Backend.RegistrationForms.FormPaths;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RegistrationFormItem
{
    public Guid? RegistrationFormId { get; internal set; }
    public string ExternalIdentifier { get; internal set; }
    public EventState State { get; internal set; }
    public string? Title { get; internal set; }
    public IEnumerable<FormSection> Sections { get; set; }

    //public string Language { get; internal set; }
    public DateTimeOffset? LastImport { get; internal set; }
    public DateTimeOffset? PendingRawFormCreated { get; internal set; }
    public Guid? PendingRawFormId { get; internal set; }
    public bool Deletable { get; set; }
}
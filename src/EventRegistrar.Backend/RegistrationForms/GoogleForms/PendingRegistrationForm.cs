namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RegistrationFormItem
{
    public Guid? RegistrationFormId { get; internal set; }
    public string ExternalIdentifier { get; internal set; }
    public EventState State { get; internal set; }

    public string Title { get; internal set; }

    //public string Language { get; internal set; }
    public DateTime? LastImport { get; internal set; }
    public DateTime? PendingRawFormCreated { get; internal set; }
    public Guid? PendingRawFormId { get; internal set; }
    public bool Deletable { get; set; }
}
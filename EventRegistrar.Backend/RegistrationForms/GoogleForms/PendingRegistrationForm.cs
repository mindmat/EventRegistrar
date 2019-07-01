using System;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class PendingRegistrationForm
    {
        public DateTime Created { get; internal set; }
        public Guid? RawRegistrationFormId { get; internal set; }
        public bool Processed { get; internal set; }
    }

    public class RegistrationFormItem
    {
        public string ExternalIdentifier { get; internal set; }
        public PendingRegistrationForm PendingRawForm { get; internal set; }
        public Guid? RegistrationFormId { get; internal set; }
        public State State { get; internal set; }
        public string Title { get; internal set; }
        public string Language { get; internal set; }
    }
}
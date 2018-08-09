using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class RawRegistrationForm : Entity
    {
        public DateTime Created { get; set; }
        public string EventAcronym { get; set; }
        public string FormExternalIdentifier { get; set; }
        public bool Processed { get; set; }
        public string ReceivedMessage { get; set; }
    }
}
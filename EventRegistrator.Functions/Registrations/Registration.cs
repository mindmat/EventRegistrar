using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.RegistrationForms;
using System;

namespace EventRegistrator.Functions.Registrations
{
    public class Registration : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public RegistrationForm RegistrationForm { get; set; }
        public string ExternalIdentifier { get; set; }
        public string RespondentEmail { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime ExternalTimestamp { get; set; }
    }
}
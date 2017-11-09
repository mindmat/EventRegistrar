using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.RegistrationForms;

namespace EventRegistrator.Functions.Registrations
{
    public class Registration : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public RegistrationForm RegistrationForm { get; set; }
        public string ExternalIdentifier { get; set; }
        public string RespondentEmail { get; set; }
        public string RespondentFirstName { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime ExternalTimestamp { get; set; }
        public string Language { get; set; }
        public decimal? Price { get; set; }
        public bool? IsWaitingList { get; set; }
        public string SoldOutMessage { get; set; }
        public bool IsPayed { get; set; }
    }
}
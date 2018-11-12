using System;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class PotentialPartnerMatch
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public bool IsWaitingList { get; set; }
        public string LastName { get; set; }
        public string MatchedPartner { get; set; }
        public string Partner { get; set; }
        public string[] Registrables { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid? RegistrationId_Partner { get; set; }
        public string State { get; set; }
    }
}
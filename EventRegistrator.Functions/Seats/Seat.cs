using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.Registrations;

namespace EventRegistrator.Functions.Seats
{
    public class Seat : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid? RegistrationId { get; set; }
        public Registration Registration { get; set; }
        public Guid? RegistrationId_Follower { get; set; }
        public Registration Registration_Follower { get; set; }
        public Registrable Registrable { get; set; }
        public string PartnerEmail { get; set; }
        public DateTime FirstPartnerJoined { get; set; }
        public bool IsWaitingList { get; set; }
        public bool IsCancelled { get; set; }
    }
}
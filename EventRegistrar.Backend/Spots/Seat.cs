using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots
{
    public class Seat : Entity
    {
        public DateTime FirstPartnerJoined { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPartnerSpot { get; set; }
        public bool IsWaitingList { get; set; }
        public string PartnerEmail { get; set; }
        public Registrable Registrable { get; set; }
        public Guid RegistrableId { get; set; }
        public Registration Registration { get; set; }
        public Registration Registration_Follower { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? RegistrationId_Follower { get; set; }
    }
}
using System;

namespace EventRegistrar.Backend.Registrables.Participants
{
    public class PlaceDisplayInfo
    {
        public RegistrationDisplayInfo Follower { get; set; }
        public bool IsOnWaitingList { get; set; }
        public bool IsPartnerRegistration { get; set; }
        public DateTime? Joined { get; set; }
        public RegistrationDisplayInfo Leader { get; set; }
        public string PlaceholderPartner { get; set; }
    }
}
using EventRegistrator.Functions.Registrations;

namespace EventRegistrator.Functions.Seats
{
    public class PlaceDisplayInfo
    {
        public bool IsPartnerRegistration { get; set; }
        public RegistrationDisplayInfo Leader { get; set; }
        public RegistrationDisplayInfo Follower { get; set; }
        public bool IsOnWaitingList { get; set; }
    }
}
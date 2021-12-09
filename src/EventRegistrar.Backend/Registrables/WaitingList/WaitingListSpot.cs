using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class WaitingListRegistration
{
    public string FirstName { get; set; }
    public Guid? Id { get; set; }
    public string LastName { get; set; }
    public bool OptionsSent { get; set; }
    public RegistrationState State { get; set; }
}

public class WaitingListSpot
{
    public WaitingListRegistration Follower { get; set; }
    public bool IsOnWaitingList { get; set; }
    public bool IsPartnerRegistration { get; set; }
    public DateTime? Joined { get; set; }
    public WaitingListRegistration Leader { get; set; }
    public string PlaceholderPartner { get; set; }
    public string RegistrableName { get; set; }
}
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEventDisplayItem
    {
        public string EventAcronym { get; set; }
        public string EventName { get; set; }
        public State EventState { get; set; }
        public UserInEventRole Role { get; set; }
        public string UserFirstName { get; set; }
        public string UserIdentifier { get; set; }
        public string UserLastName { get; set; }
    }
}
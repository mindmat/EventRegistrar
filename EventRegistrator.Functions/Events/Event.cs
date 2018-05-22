using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Events
{
    public class Event : Entity
    {
        public string AccountIban { get; set; }
        public string Acronym { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public State State { get; set; }
        public string TwilioAccountSid { get; set; }
    }
}
using EventRegistrator.Functions.Infrastructure.DataAccess;
using System.Collections.Generic;

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
        public ICollection<UserInEvent> Users { get; set; }
    }
}
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Users
{
    public class User : Entity
    {
        public ICollection<UserInEvent> Events { get; set; }
        public string IdentityProvider { get; set; }
        public string IdentityProviderUserIdentifier { get; set; }
        public string Name { get; set; }
    }
}
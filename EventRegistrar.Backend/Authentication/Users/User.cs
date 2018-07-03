using System.Collections.Generic;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Authentication.Users
{
    public class User : Entity
    {
        public string Email { get; set; }
        public ICollection<UserInEvent> Events { get; set; }
        public string FirstName { get; set; }
        public IdentityProvider IdentityProvider { get; set; }
        public string IdentityProviderUserIdentifier { get; set; }
        public string LastName { get; set; }
    }
}
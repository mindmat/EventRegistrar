using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using System.Collections.Generic;

namespace EventRegistrator.Functions.Users
{
    public class User : Entity
    {
        public ICollection<UserInEvent> Events { get; set; }
        public string IdentityProvider { get; set; }
        public string IdentityProviderUserIdentifier { get; set; }
        public string Name { get; set; }
    }
}
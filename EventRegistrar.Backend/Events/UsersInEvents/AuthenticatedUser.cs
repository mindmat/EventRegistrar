using System;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}
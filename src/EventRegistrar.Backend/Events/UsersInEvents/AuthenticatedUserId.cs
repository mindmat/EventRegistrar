using System;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AuthenticatedUserId
    {
        public AuthenticatedUserId(Guid? userId)
        {
            UserId = userId;
        }

        public Guid? UserId { get; }
    }
}
using System;
using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authorization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CheckRoleAttribute : Attribute
    {
        public CheckRoleAttribute(UserInEventRole role)
        {
            Role = role;
        }

        public UserInEventRole Role { get; }
    }
}
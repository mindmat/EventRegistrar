using EventRegistrar.Backend.Events.UsersInEvents;
using System;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Authorization
{
    public interface IAuthorizationChecker
    {
        Task ThrowIfUserIsNotInEventRole(Guid eventId, UserInEventRole role);
    }
}
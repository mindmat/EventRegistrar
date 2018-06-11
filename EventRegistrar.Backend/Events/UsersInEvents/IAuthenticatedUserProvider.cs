using System;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public interface IAuthenticatedUserProvider
    {
        Task<Guid> GetAuthenticatedUserId();
    }
}
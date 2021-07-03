using System;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Authorization
{
    public interface IAuthorizationChecker
    {
        Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName);
        Task<bool> UserHasRight(Guid eventId, string requestTypeName);
    }
}
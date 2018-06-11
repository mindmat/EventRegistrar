using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<Guid> GetAuthenticatedUserId()
        {
            //_httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault()
            return Task.FromResult(Guid.Empty);
        }
    }
}
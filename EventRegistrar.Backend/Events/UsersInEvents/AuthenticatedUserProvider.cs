﻿using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityProvider _identityProvider;
        private readonly IQueryable<User> _users;

        public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
                                         IIdentityProvider identityProvider,
                                         IQueryable<User> users)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityProvider = identityProvider;
            _users = users;
        }

        public async Task<Guid> GetAuthenticatedUserId()
        {
            var identifier = _identityProvider.GetIdentifier(_httpContextAccessor);
            var user = await _users.FirstOrDefaultAsync(usr => usr.IdentityProvider == _identityProvider.Provider
                                                            && usr.IdentityProviderUserIdentifier == identifier);
            if (user == null)
            {
                //throw new AuthenticationException($"There is no user {identifier} registered (provider {_identityProvider.Provider})");
                return new Guid();
            }

            return user.Id; //new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC"));
        }
    }
}
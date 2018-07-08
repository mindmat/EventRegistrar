﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EventRegistrar.Backend.Authorization
{
    internal class AuthorizationChecker : IAuthorizationChecker
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IRightsOfEventRoleProvider _rightsOfEventRoleProvider;
        private readonly TimeSpan _slidingExpiration = new TimeSpan(1, 0, 0, 0);
        private readonly AuthenticatedUserId _user;
        private readonly IQueryable<UserInEvent> _usersInEvents;

        public AuthorizationChecker(AuthenticatedUserId user,
                                    IQueryable<UserInEvent> usersInEventsInEvents,
                                    IMemoryCache memoryCache,
                                    IRightsOfEventRoleProvider rightsOfEventRoleProvider)
        {
            _user = user;
            _usersInEvents = usersInEventsInEvents;
            _memoryCache = memoryCache;
            _rightsOfEventRoleProvider = rightsOfEventRoleProvider;
        }

        public async Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName)
        {
            if (!_user.UserId.HasValue)
            {
            }
            else
            {
                var key = new UserInEventCacheKey(_user.UserId.Value, eventId);
                var rightsOfUserInEvent = await _memoryCache.GetOrCreateAsync(key, entry => GetRightsOfUserInEvent(entry, eventId));

                if (!rightsOfUserInEvent.Contains(requestTypeName))
                {
                    throw new UnauthorizedAccessException($"You ({_user.UserId}) are not authorized for {requestTypeName} in event {eventId}");
                }
            }
        }

        private async Task<HashSet<string>> GetRightsOfUserInEvent(ICacheEntry entry, Guid eventId)
        {
            entry.SlidingExpiration = _slidingExpiration;
            var usersRolesInEvent = await _usersInEvents.Where(uie => uie.UserId == _user.UserId &&
                                                                      uie.EventId == eventId)
                                                        .Select(uie => uie.Role)
                                                        .ToListAsync();

            return _rightsOfEventRoleProvider.GetRightsOfEventRoles(eventId, usersRolesInEvent).ToHashSet();
        }
    }
}
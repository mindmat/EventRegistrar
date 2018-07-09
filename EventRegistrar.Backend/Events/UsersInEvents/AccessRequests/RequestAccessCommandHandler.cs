using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class RequestAccessCommandHandler : IRequestHandler<RequestAccessCommand, Guid>
    {
        private readonly IRepository<AccessToEventRequest> _accessRequests;
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
        private readonly AuthenticatedUserId _user;

        public RequestAccessCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                           IEventAcronymResolver acronymResolver,
                                           AuthenticatedUserId user,
                                           IAuthenticatedUserProvider authenticatedUserProvider)
        {
            _accessRequests = accessRequests;
            _acronymResolver = acronymResolver;
            _user = user;
            _authenticatedUserProvider = authenticatedUserProvider;
        }

        public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var request = await _accessRequests.FirstOrDefaultAsync(req => req.EventId == eventId
                                                                        && req.UserId_Requestor == _user.UserId
                                                                        && (!req.Response.HasValue || req.Response == RequestResponse.Granted), cancellationToken);

            if (request == null)
            {
                var user = _authenticatedUserProvider.GetAuthenticatedUser();
                request = new AccessToEventRequest
                {
                    Id = Guid.NewGuid(),
                    UserId_Requestor = _user.UserId,
                    IdentityProvider = user.IdentityProvider,
                    Identifier = user.IdentityProviderUserIdentifier,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    RequestText = command.RequestText,
                    EventId = eventId,
                    RequestReceived = DateTime.UtcNow
                };
                await _accessRequests.InsertOrUpdateEntity(request, cancellationToken);
            }

            return request.Id;
        }
    }
}
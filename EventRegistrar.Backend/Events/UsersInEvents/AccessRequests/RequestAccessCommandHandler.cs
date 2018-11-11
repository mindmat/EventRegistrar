using System;
using System.Linq;
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
        private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
        private readonly AuthenticatedUserId _user;

        public RequestAccessCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                           AuthenticatedUserId user,
                                           IAuthenticatedUserProvider authenticatedUserProvider)
        {
            _accessRequests = accessRequests;
            _user = user;
            _authenticatedUserProvider = authenticatedUserProvider;
        }

        public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
        {
            var requestExpression = _accessRequests.Where(req => req.EventId == command.EventId
                                                              && (!req.Response.HasValue || req.Response == RequestResponse.Granted));
            var existingUserId = await _authenticatedUserProvider.GetAuthenticatedUserId();
            if (existingUserId.HasValue)
            {
                requestExpression = requestExpression.Where(req => req.UserId_Requestor == existingUserId.Value);
            }
            else
            {
                var authenticatedUser = _authenticatedUserProvider.GetAuthenticatedUser();
                if (authenticatedUser?.IdentityProviderUserIdentifier == null)
                {
                    throw new ArgumentException("You are not authenticated");
                }
                requestExpression = requestExpression.Where(req => req.IdentityProvider == authenticatedUser.IdentityProvider
                                                                 && req.Identifier == authenticatedUser.IdentityProviderUserIdentifier);
            }

            var request = await requestExpression.FirstOrDefaultAsync(cancellationToken);

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
                    EventId = command.EventId,
                    RequestReceived = DateTime.UtcNow
                };
                await _accessRequests.InsertOrUpdateEntity(request, cancellationToken);
            }

            return request.Id;
        }
    }
}
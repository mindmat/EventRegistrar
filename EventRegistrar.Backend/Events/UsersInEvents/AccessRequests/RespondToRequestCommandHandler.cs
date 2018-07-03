using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class RespondToRequestCommandHandler : IRequestHandler<RespondToRequestCommand>
    {
        private readonly IRepository<AccessToEventRequest> _accessRequests;
        private readonly IRepository<User> _users;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public RespondToRequestCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                              IRepository<UserInEvent> usersInEvents,
                                              IRepository<User> users)
        {
            _accessRequests = accessRequests;
            _usersInEvents = usersInEvents;
            _users = users;
        }

        public async Task<Unit> Handle(RespondToRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _accessRequests.FirstAsync(req => req.Id == command.AccessToEventRequestId, cancellationToken);
            if (request.Response.HasValue)
            {
                throw new ArgumentException("Request has already been answered");
            }

            request.Response = command.Response;
            request.ResponseText = command.ResponseText;
            if (request.Response == RequestResponse.Granted)
            {
                var userId = await CreateUserIfNecessary(request);
                var userInEvent = new UserInEvent
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EventId = request.EventId,
                    Role = command.Role
                };
                await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
            }

            return Unit.Value;
        }

        private async Task<Guid> CreateUserIfNecessary(AccessToEventRequest request)
        {
            if (request.UserId_Requestor.HasValue)
            {
                return request.UserId_Requestor.Value;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                IdentityProvider = request.IdentityProvider,
                IdentityProviderUserIdentifier = request.Identifier,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };
            await _users.InsertOrUpdateEntity(user);

            return user.Id;
        }
    }
}
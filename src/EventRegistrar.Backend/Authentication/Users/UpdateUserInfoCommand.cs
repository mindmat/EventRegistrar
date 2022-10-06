using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Authentication.Users;

public class UpdateUserInfoCommand : IRequest
{
    public IdentityProvider Provider { get; set; }
    public string Identifier { get; set; } = null!;
}

public class UpdateUserInfoCommandHandler : IRequestHandler<UpdateUserInfoCommand>
{
    private readonly IIdentityProvider _identityProvider;
    private readonly IRepository<AccessToEventRequest> _accessRequests;

    public UpdateUserInfoCommandHandler(IIdentityProvider identityProvider,
                                        IRepository<AccessToEventRequest> accessRequests)
    {
        _identityProvider = identityProvider;
        _accessRequests = accessRequests;
    }

    public async Task<Unit> Handle(UpdateUserInfoCommand command, CancellationToken cancellationToken)
    {
        var requests = await _accessRequests.Where(arq => arq.IdentityProvider == command.Provider
                                                       && arq.Identifier == command.Identifier
                                                       && (arq.FirstName == null || arq.LastName == null || arq.Email == null))
                                            .AsTracking()
                                            .ToListAsync(cancellationToken);
        if (!requests.Any())
        {
            return Unit.Value;
        }

        var userDetails = await _identityProvider.GetUserDetails(command.Identifier);
        if (userDetails != null)
        {
            requests.ForEach(arq =>
            {
                arq.FirstName = userDetails.FirstName;
                arq.LastName = userDetails.LastName;
                arq.Email = userDetails.Email;
            });
        }

        return Unit.Value;
    }
}
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Authentication.Users;

public class UpdateUserInfoCommand : IRequest
{
    public IdentityProvider Provider { get; set; }
    public string? Identifier { get; set; } = null!;
}

public class UpdateUserInfoCommandHandler : IRequestHandler<UpdateUserInfoCommand>
{
    private readonly IIdentityProvider _identityProvider;
    private readonly IRepository<AccessToEventRequest> _accessRequests;
    private readonly IRepository<User> _users;

    public UpdateUserInfoCommandHandler(IIdentityProvider identityProvider,
                                        IRepository<AccessToEventRequest> accessRequests,
                                        IRepository<User> users)
    {
        _identityProvider = identityProvider;
        _accessRequests = accessRequests;
        _users = users;
    }

    public async Task<Unit> Handle(UpdateUserInfoCommand command, CancellationToken cancellationToken)
    {
        var requests = await _accessRequests.Where(arq => arq.IdentityProvider == command.Provider
                                                       && (arq.FirstName == null || arq.LastName == null || arq.Email == null || arq.AvatarUrl == null))
                                            .WhereIf(command.Identifier != null, arq => arq.Identifier == command.Identifier)
                                            .AsTracking()
                                            .ToListAsync(cancellationToken);
        var users = await _users.Where(usr => usr.IdentityProvider == command.Provider
                                           && (usr.FirstName == null || usr.LastName == null || usr.Email == null || usr.AvatarUrl == null))
                                .WhereIf(command.Identifier != null, usr => usr.IdentityProviderUserIdentifier == command.Identifier)
                                .AsTracking()
                                .ToListAsync(cancellationToken);
        if (!requests.Any() && !users.Any())
        {
            return Unit.Value;
        }

        var identifiers = requests.Select(arq => arq.Identifier)
                                  .Concat(users.Select(usr => usr.IdentityProviderUserIdentifier))
                                  .Distinct();
        foreach (var identifier in identifiers.Where(id => id != null))
        {
            var userDetails = await _identityProvider.GetUserDetails(identifier!);
            if (userDetails != null)
            {
                requests.Where(arq => arq.Identifier == identifier)
                        .ForEach(arq =>
                        {
                            arq.FirstName = userDetails.FirstName ?? arq.FirstName;
                            arq.LastName = userDetails.LastName ?? arq.LastName;
                            arq.Email = userDetails.Email ?? arq.Email;
                            arq.AvatarUrl = userDetails.AvatarUrl ?? arq.AvatarUrl;
                        });
                users.Where(usr => usr.IdentityProviderUserIdentifier == identifier)
                     .ForEach(usr =>
                     {
                         usr.FirstName = userDetails.FirstName ?? usr.FirstName;
                         usr.LastName = userDetails.LastName ?? usr.LastName;
                         usr.Email = userDetails.Email ?? usr.Email;
                         usr.AvatarUrl = userDetails.AvatarUrl ?? usr.AvatarUrl;
                     });
            }
        }

        return Unit.Value;
    }
}
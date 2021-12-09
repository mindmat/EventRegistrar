namespace EventRegistrar.Backend.Events.UsersInEvents;

public interface IAuthenticatedUserProvider
{
    AuthenticatedUser GetAuthenticatedUser();

    Task<Guid?> GetAuthenticatedUserId();
}
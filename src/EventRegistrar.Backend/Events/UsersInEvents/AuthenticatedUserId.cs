namespace EventRegistrar.Backend.Events.UsersInEvents;

public class AuthenticatedUserId(Guid? userId)
{
    public Guid? UserId { get; } = userId;
}
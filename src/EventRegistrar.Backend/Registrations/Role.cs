namespace EventRegistrar.Backend.Registrations;

public enum Role
{
    Leader = 1,
    Follower = 2
}

public static class RoleExtensions
{
    public static Role GetOtherRole(this Role role)
    {
        return role == Role.Leader
            ? Role.Follower
            : Role.Leader;
    }
}
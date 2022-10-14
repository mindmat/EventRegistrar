namespace EventRegistrar.Backend.Authentication.Users;

public class ExternalUserDetails
{
    public IdentityProvider IdentityProvider { get; init; }
    public string ExternalIdentifier { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
}
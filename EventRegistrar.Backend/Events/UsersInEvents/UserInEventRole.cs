namespace EventRegistrar.Backend.Events.UsersInEvents
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks>Convention: Roles with higher numbers include roles with lower numbers</remarks>
    public enum UserInEventRole
    {
        Reader = 1,
        Writer = 2,
        Admin = 3
    }
}
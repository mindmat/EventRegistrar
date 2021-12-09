namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class AccessRequestOfEvent
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public Guid Id { get; set; }
    public string LastName { get; set; }
    public DateTime RequestReceived { get; set; }
    public string RequestText { get; set; }
}
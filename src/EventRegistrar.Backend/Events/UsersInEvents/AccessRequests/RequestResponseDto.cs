namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class RequestResponseDto
{
    public RequestResponse Response { get; set; }
    public UserInEventRole Role { get; set; }
    public string Text { get; set; }
}
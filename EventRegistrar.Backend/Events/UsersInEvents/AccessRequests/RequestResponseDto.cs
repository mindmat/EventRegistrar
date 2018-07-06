using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

namespace EventRegistrar.Backend.Events
{
    public class RequestResponseDto
    {
        public RequestResponse Response { get; set; }
        public UserInEventRole Role { get; set; }
        public string Text { get; set; }
    }
}
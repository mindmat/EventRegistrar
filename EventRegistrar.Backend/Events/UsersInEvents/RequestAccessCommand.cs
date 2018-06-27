using MediatR;

namespace EventRegistrar.Backend.Events
{
    public class RequestAccessCommand : IRequest<Unit>
    {
        public string EventAcronym { get; set; }
    }
}
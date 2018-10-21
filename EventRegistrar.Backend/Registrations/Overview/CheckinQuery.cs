using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Overview
{
    public class CheckinQuery : IRequest<CheckinView>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}
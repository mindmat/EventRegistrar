using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Seats
{
    public class SpotsOfRegistrationQuery : IRequest<IEnumerable<Spot>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
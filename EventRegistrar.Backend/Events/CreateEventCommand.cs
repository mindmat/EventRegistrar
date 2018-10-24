using System;
using MediatR;

namespace EventRegistrar.Backend.Events
{
    public class CreateEventCommand : IRequest
    {
        public string Acronym { get; set; }
        public Guid? EventId_CopyFrom { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
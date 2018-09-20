using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Spots
{
    public class SpotRemoved : Event
    {
        public Guid RegistrableId { get; set; }
    }
}
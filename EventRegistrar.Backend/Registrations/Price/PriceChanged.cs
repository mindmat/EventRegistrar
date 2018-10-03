using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class PriceChanged : Event
    {
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public Guid RegistrationId { get; set; }
    }
}
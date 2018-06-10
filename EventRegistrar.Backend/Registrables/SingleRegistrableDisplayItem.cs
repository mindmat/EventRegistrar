using System;

namespace EventRegistrar.Backend.Registrables
{
    public class SingleRegistrableDisplayItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? SpotsAvailable { get; set; }
    }
}
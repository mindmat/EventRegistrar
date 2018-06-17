using System;

namespace EventRegistrar.Backend.Registrables
{
    public class SingleRegistrableDisplayItem
    {
        public int Accepted { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? OnWaitingList { get; set; }
        public int? SpotsAvailable { get; set; }
    }
}
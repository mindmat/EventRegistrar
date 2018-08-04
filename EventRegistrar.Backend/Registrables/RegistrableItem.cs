using System;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrableItem
    {
        public bool HasWaitingList { get; set; }
        public Guid Id { get; set; }
        public bool IsDoubleRegistrable { get; set; }
        public string Name { get; set; }
        public int? ShowInMailListOrder { get; set; }
        public int? SortKey { get; set; }
    }
}
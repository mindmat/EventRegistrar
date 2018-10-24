using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrables.Compositions
{
    public class RegistrableComposition : Entity
    {
        public Registrable Registrable { get; set; }
        public Registrable Registrable_Contains { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid RegistrableId_Contains { get; set; }
    }
}
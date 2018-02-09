using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrables
{
    public class RegistrableComposition : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrableId_Contains { get; set; }

        public Registrable Registrable { get; set; }
        public Registrable Registrable_Contains { get; set; }
    }
}
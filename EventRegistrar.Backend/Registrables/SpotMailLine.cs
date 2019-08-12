using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrables
{
    public class SpotMailLine : Entity
    {
        public Guid RegistrableId { get; set; }
        public string Language { get; set; }
        public string Text { get; set; }
    }
}

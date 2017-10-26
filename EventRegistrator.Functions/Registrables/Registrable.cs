using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Seats;
using System;
using System.Collections.Generic;

namespace EventRegistrator.Functions.Registrables
{
    public class Registrable : Entity
    {
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public int? MaximumSingleSeats { get; set; }
        public int? MaximumDoubleSeats { get; set; }
        public int? MaximumAllowedImbalance { get; set; }
        public ICollection<QuestionOptionToRegistrableMapping> QuestionOptionMappings { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public int? ShowInMailListOrder { get; set; }
    }
}
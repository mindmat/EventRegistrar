using System;
using System.Collections.Generic;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Seats;

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
        public decimal? Price { get; set; }
        public bool HasWaitingList { get; set; }
        public bool IsCore { get; set; }
        public ICollection<Reduction> Reductions { get; set; }
        public string CheckinListColumn { get; set; }
        public ICollection<RegistrableComposition> Compositions { get; set; }
    }
}
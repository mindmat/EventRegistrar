using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables.Compositions;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables
{
    public class Registrable : Entity
    {
        public string CheckinListColumn { get; set; }

        public ICollection<RegistrableComposition> Compositions { get; set; }
        public Guid EventId { get; set; }

        public bool HasWaitingList { get; set; }
        public bool IsCore { get; set; }
        public int? MaximumAllowedImbalance { get; set; }
        public int? MaximumDoubleSeats { get; set; }
        public int? MaximumSingleSeats { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public ICollection<QuestionOptionToRegistrableMapping> QuestionOptionMappings { get; set; }
        public ICollection<Reduction> Reductions { get; set; }
        public List<Seat> Seats { get; set; }
        public int? ShowInMailListOrder { get; set; }
    }
}
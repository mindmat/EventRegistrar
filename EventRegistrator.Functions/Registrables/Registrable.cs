using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrables
{
    public class Registrable : Entity
    {
        
        public Guid EventId { get; set; }
        public Guid QuestionOptionId { get; set; }
        public string Name { get; set; }
        public int? MaximumSingleSeats { get; set; }
        public int? MaximumDoubleSeats { get; set; }
    }
}
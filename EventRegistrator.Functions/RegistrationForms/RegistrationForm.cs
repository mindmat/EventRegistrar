using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventRegistrator.Functions.RegistrationForms
{
    [Table(nameof(RegistrationForm))]
    public class RegistrationForm : Entity
    {
        public string ExternalIdentifier { get; set; }

        public string Title { get; set; }

        public ICollection<Question> Questions { get; set; }

        public Guid? EventId { get; set; }
        public Event Event { get; set; }
        public State State { get; set; }
    }
}
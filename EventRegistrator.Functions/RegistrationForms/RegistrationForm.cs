using System;
using System.Collections.Generic;
using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.RegistrationForms
{
    public class RegistrationForm : Entity
    {
        public string ExternalIdentifier { get; set; }
        public string Title { get; set; }
        public ICollection<Question> Questions { get; set; }
        public Guid? QuestionId_FirstName { get; set; }
        public Guid? QuestionId_LastName { get; set; }
        public Guid? EventId { get; set; }
        public Event Event { get; set; }
        public State State { get; set; }
        public string Language { get; set; }
    }
}
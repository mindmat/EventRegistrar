using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationForm : Entity
    {
        public Event Event { get; set; }
        public Guid? EventId { get; set; }
        public string ExternalIdentifier { get; set; }
        public string Language { get; set; }

        public string ProcessConfigurationJson { get; set; }
        public Guid? QuestionId_FirstName { get; set; }
        public Guid? QuestionId_LastName { get; set; }
        public Guid? QuestionId_Phone { get; set; }
        public Guid? QuestionId_Remarks { get; set; }
        public ICollection<Question> Questions { get; set; }
        public State State { get; set; }
        public string Title { get; set; }
    }
}
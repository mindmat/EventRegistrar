using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionOption : Entity
    {
        public string Answer { get; set; }
        public Question Question { get; set; }
        public Guid QuestionId { get; set; }
        public IEnumerable<QuestionOptionToRegistrableMapping> Registrables { get; set; }
    }
}
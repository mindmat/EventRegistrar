using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class QuestionOption : Entity
    {
        public string Answer { get; set; }
        public Guid QuestionId { get; set; }
        //public Question Question { get; set; }
    }
}
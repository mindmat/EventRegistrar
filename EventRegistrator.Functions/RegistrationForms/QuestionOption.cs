using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.RegistrationForms
{
    public class QuestionOption : Entity
    {
        public string Answer { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
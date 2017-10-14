using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;

namespace EventRegistrator.Functions.RegistrationForms
{
    public class Question : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public RegistrationForm RegistrationForm { get; set; }
        public int ExternalId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public QuestionType Type { get; set; }
        public ICollection<QuestionOption> QuestionOptions { get; set; }
    }
}
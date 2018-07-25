using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class Question : Entity
    {
        //public RegistrationForm RegistrationForm { get; set; }
        public int ExternalId { get; set; }

        public int Index { get; set; }
        public Guid RegistrationFormId { get; set; }

        //public ICollection<QuestionOption> QuestionOptions { get; set; }
        public string TemplateKey { get; set; }

        public string Title { get; set; }
        public QuestionType Type { get; set; }
    }
}
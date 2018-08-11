using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

namespace EventRegistrar.Backend.Registrations.Responses
{
    public class Response : Entity
    {
        public Question Question { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? QuestionOptionId { get; set; }
        public Registration Registration { get; set; }

        public Guid RegistrationId { get; set; }
        public string ResponseString { get; set; }
        //public QuestionOptionToRegistrableMapping RegistrableMappings { get; set; }
    }
}
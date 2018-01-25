using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.RegistrationForms;
using System;

namespace EventRegistrator.Functions.Registrations
{
    public class Response : Entity
    {
        public Guid? QuestionId { get; set; }
        public string ResponseString { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid? QuestionOptionId { get; set; }
        public Question Question { get; set; }
        public Registration Registration { get; set; }

        //public QuestionOptionToRegistrableMapping RegistrableMappings { get; set; }
    }
}
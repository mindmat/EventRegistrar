using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionOptionToRegistrableMapping : Entity
    {
        public Guid? QuestionId_Partner { get; set; }
        public QuestionOption QuestionOption { get; set; }
        public Guid QuestionOptionId { get; set; }
        public Guid? QuestionOptionId_Follower { get; set; }
        public Guid? QuestionOptionId_Leader { get; set; }
        public Registrable Registrable { get; set; }
        public Guid RegistrableId { get; set; }
    }
}
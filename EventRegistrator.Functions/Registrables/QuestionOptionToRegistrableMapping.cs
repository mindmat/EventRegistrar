using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.Registrables
{
    public class QuestionOptionToRegistrableMapping : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid QuestionOptionId { get; set; }
        public Registrable Registrable { get; set; }
    }
}
using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.Registrables
{
    public class QuestionOptionToRegistrableMapping : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid QuestionOptionId { get; set; }
        public Guid? QuestionId_PartnerEmail { get; set; }
        public Guid? QuestionOptionId_Leader { get; set; }
        public Guid? QuestionOptionId_Follower { get; set; }
        public Registrable Registrable { get; set; }
    }
}
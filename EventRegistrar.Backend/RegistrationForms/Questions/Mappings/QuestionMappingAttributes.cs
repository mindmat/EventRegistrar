using System;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class QuestionMappingAttributes
    {
        public Guid? QuestionId_Partner { get; set; }
        public Guid? QuestionOptionId_Leader { get; set; }
        public Guid? QuestionOptionId_Follower { get; set; }
    }
}

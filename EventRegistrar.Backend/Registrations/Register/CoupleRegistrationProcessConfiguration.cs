using System;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class CoupleRegistrationProcessConfiguration : IRegistrationProcessConfiguration
    {
        public string Description { get; set; }
        public IEnumerable<(Guid QuestionOptionId, string Language)>? LanguageMappings { get; set; }
        public Guid QuestionId_Follower_Email { get; set; }
        public Guid QuestionId_Follower_FirstName { get; set; }
        public Guid QuestionId_Follower_LastName { get; set; }
        public Guid? QuestionId_Follower_Phone { get; set; }
        public Guid QuestionId_Leader_Email { get; set; }
        public Guid QuestionId_Leader_FirstName { get; set; }
        public Guid QuestionId_Leader_LastName { get; set; }
        public Guid? QuestionId_Leader_Phone { get; set; }
        public Guid QuestionOptionId_Trigger { get; set; }
        public IEnumerable<(Guid QuestionOptionId, Role Role, Guid RegistrableId)>? RoleSpecificMappings { get; set; }
    }
}
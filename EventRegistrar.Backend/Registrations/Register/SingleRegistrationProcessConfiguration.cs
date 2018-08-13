﻿using System;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class SingleRegistrationProcessConfiguration : IRegistrationProcessConfiguration
    {
        public string Description { get; set; }
        public Guid QuestionId_Email { get; set; }
        public Guid QuestionId_FirstName { get; set; }
        public Guid QuestionId_LastName { get; set; }
        public Guid? QuestionId_Phone { get; set; }
        public Guid QuestionOptionId_Follower { get; set; }
        public Guid QuestionOptionId_Leader { get; set; }
        public Guid QuestionOptionId_Trigger { get; set; }
    }
}
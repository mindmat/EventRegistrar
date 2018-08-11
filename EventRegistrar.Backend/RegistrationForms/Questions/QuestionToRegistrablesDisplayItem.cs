using System;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesDisplayItem
    {
        public string Answer { get; set; }
        public string Question { get; set; }
        public Guid QuestionOptionId { get; set; }
        public Guid RegistrableId { get; set; }
        public string RegistrableName { get; set; }
    }
}
using System.Collections.Generic;

namespace EventRegistrator.Functions.RegistrationForms
{
    public class FormDescription
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<QuestionDescription> Questions { get; set; }
    }
}
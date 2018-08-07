using System.Collections.Generic;

namespace EventRegistrar.Functions.GoogleForms
{
    public class FormDescription
    {
        public IEnumerable<QuestionDescription> Questions { get; set; }
        public string Title { get; set; }
    }
}
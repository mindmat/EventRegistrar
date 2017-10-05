using System.Collections.Generic;

namespace EventRegistrator.Functions.GoogleForms
{
    public class FormDescription
    {
        public string Identifier { get; set; }
        public string Title { get; set; }
        public IEnumerable<QuestionDescription> Questions { get; set; }
    }
}
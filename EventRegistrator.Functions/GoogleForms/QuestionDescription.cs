namespace EventRegistrator.Functions.GoogleForms
{
    public class QuestionDescription
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public QuestionType Type { get; set; }
        public string[] Choices { get; set; }
    }
}
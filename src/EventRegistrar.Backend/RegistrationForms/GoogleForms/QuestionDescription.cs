namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class QuestionDescription
{
    public int Id { get; set; }
    public int Index { get; set; }
    public string Title { get; set; }
    public GoogleQuestionType Type { get; set; }
    public string[] Choices { get; set; }
}
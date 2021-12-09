namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class FormDescription
{
    public IEnumerable<QuestionDescription> Questions { get; set; }
    public string Title { get; set; }
}
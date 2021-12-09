namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class ResponseData
{
    public int QuestionExternalId { get; set; }
    public string Response { get; set; }
    public string[] Responses { get; set; }
}
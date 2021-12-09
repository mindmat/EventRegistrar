namespace EventRegistrar.Backend.Mailing;

public class MailEventDisplayItem
{
    public string Email { get; set; }
    public MailState State { get; set; }
    public string StateText { get; set; }
    public DateTime When { get; set; }
}
namespace EventRegistrar.Backend.Mailing;

public class MailDisplayItem
{
    public string ContentHtml { get; set; }
    public DateTime Created { get; set; }
    public IEnumerable<MailEventDisplayItem> Events { get; set; }
    public Guid Id { get; set; }
    public string Recipients { get; set; }
    public string SenderMail { get; set; }
    public string SenderName { get; set; }
    public MailState? State { get; set; }
    public string Subject { get; set; }
    public bool Withhold { get; set; }
}
namespace EventRegistrar.Backend.Mailing.Templates;

public class BulkMailTemplateItem
{
    public MailingAudience? Audience { get; set; }
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Language { get; set; }
    public int MailsReadyCount { get; set; }
    public int MailsSentCount { get; set; }
    public string SenderMail { get; set; }
    public string SenderName { get; set; }
    public string Subject { get; set; }
    public string Template { get; set; }
    public MailType? Type { get; set; }
    public bool ReleaseImmediately { get; set; }
}
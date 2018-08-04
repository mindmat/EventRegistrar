namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplateItem
    {
        public MailingAudience? Audience { get; set; }
        public string Key { get; set; }
        public string Language { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
    }
}
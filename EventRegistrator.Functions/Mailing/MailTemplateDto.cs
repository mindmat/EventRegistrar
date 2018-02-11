namespace EventRegistrator.Functions.Mailing
{
    public class MailTemplateDto
    {
        public string Language { get; set; }
        public string Template { get; set; }
        public string Subject { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string Key { get; set; }
    }
}
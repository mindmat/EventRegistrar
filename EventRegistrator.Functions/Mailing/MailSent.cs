using SendGrid.Helpers.Mail;

namespace EventRegistrator.Functions.Mailing
{
    public class MailSent
    {
        private SendGridMessage msg;

        public MailSent(SendGridMessage msg)
        {
            Content = msg.PlainTextContent;
            HtmlContent = msg.HtmlContent;
            Subject = msg.Subject;
            From = msg.From.Email;
        }

        public string From { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }
        public string HtmlContent { get; set; }
    }
}
namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTypeItem
    {
        public string BulkMailKey { get; set; }
        public MailType? Type { get; set; }
        public string UserText { get; set; }
    }
}
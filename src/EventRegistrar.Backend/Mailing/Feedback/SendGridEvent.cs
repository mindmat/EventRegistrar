using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    /// <see cref="https://sendgrid.com/docs/API_Reference/Webhooks/event.html"/>
    public class SendGridEvent
    {
        public string Email { get; set; }

        //public string Category { get; set; }
        public string Event { get; set; }

        public int Id { get; set; }
        public string Ip { get; set; }

        // our custom field to match to a sent mail
        public string MailId { get; set; }

        public string Reason { get; set; }
        public string Sg_event_id { get; set; }
        public string Sg_message_id { get; set; }

        [JsonProperty("smtp-id")]
        public string Smtp_id { get; set; }

        public string Status { get; set; }
        public int Timestamp { get; set; }
        public string Type { get; set; }
        public int Uid { get; set; }
        public string Url { get; set; }
        public string Useragent { get; set; }
    }
}
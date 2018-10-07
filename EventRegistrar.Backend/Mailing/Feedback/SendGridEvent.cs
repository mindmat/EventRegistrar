using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class SendGridEvent
    {
        //public string Category { get; set; }

        // Docs: https://sendgrid.com/docs/API_Reference/Webhooks/event.html
        public string Email { get; set; }

        public string Event { get; set; }
        public int Id { get; set; }
        public string Ip { get; set; }

        // Add your custom fields here
        public string Purchase { get; set; }

        public string Reason { get; set; }
        public string Sendgrid_event_id { get; set; }
        public string Sg_message_id { get; set; }

        [JsonProperty("smtp-id")] // switched to underscore for consistancy
        public string Smtp_id { get; set; }

        public string Status { get; set; }
        public int Timestamp { get; set; }
        public string Type { get; set; }
        public int Uid { get; set; }
        public string Url { get; set; }
        public string Useragent { get; set; }
        // this is a custom field sent by our tester
    }
}
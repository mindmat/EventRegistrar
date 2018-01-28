using System;

namespace EventRegistrator.Functions.Reminders
{
    public class SendReminderCommand
    {
        public Guid RegistrationId { get; set; }
        public bool Withhold { get; set; }
    }
}
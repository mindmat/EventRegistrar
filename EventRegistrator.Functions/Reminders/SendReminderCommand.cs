using System;

namespace EventRegistrator.Functions.Reminders
{
    public class SendReminderCommand
    {
        public Guid? RegistrationId { get; set; }
        public int? GracePeriodInDays { get; set; }
        public Guid EventId { get; set; }
    }
}
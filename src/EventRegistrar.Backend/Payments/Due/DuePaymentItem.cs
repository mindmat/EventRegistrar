using System;

namespace EventRegistrar.Backend.Payments.Due
{
    public class DuePaymentItem
    {
        public SentMailDto AcceptedMail { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public decimal? Paid { get; set; }
        public string PhoneNormalized { get; set; }
        public decimal? Price { get; set; }
        public DateTime ReceivedAt { get; set; }
        public bool Reminder1Due { get; set; }
        public SentMailDto Reminder1Mail { get; set; }
        public bool Reminder2Due { get; set; }
        public SentMailDto Reminder2Mail { get; set; }
        public int ReminderLevel { get; set; }
        public bool ReminderSmsPossible { get; set; }
        public DateTime? ReminderSmsSent { get; set; }
    }
}
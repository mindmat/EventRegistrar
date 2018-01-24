using System;
using System.Collections.Generic;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Payments;
using EventRegistrator.Functions.RegistrationForms;
using EventRegistrator.Functions.Seats;

namespace EventRegistrator.Functions.Registrations
{
    public class Registration : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public RegistrationForm RegistrationForm { get; set; }
        public string ExternalIdentifier { get; set; }
        public string RespondentEmail { get; set; }
        public string RespondentFirstName { get; set; }
        public string RespondentLastName { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime ExternalTimestamp { get; set; }
        public string Language { get; set; }
        public decimal? Price { get; set; }
        public bool? IsWaitingList { get; set; }
        public string SoldOutMessage { get; set; }
        public RegistrationState State { get; set; }
        public ICollection<PaymentAssignment> Payments { get; set; }
        public ICollection<MailToRegistration> Mails { get; set; }
        public int ReminderLevel { get; set; }
        public bool FallbackToPartyPass { get; set; }
        public ICollection<Seat> Seats_AsLeader { get; set; }
        public ICollection<Seat> Seats_AsFollower { get; set; }
    }
}
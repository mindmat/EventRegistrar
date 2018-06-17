using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Seats;

namespace EventRegistrar.Backend.Registrations
{
    public class Registration : Entity
    {
        public DateTime? AdmittedAt { get; set; }
        public Guid EventId { get; set; }
        public string ExternalIdentifier { get; set; }
        public DateTime ExternalTimestamp { get; set; }
        public bool FallbackToPartyPass { get; set; }
        public bool? IsWaitingList { get; set; }
        public string Language { get; set; }
        public string Phone { get; set; }
        public string PhoneNormalized { get; set; }
        public decimal? Price { get; set; }
        public DateTime ReceivedAt { get; set; }
        public RegistrationForm RegistrationForm { get; set; }
        public Guid RegistrationFormId { get; set; }
        public string Remarks { get; set; }
        public bool RemarksProcessed { get; set; }

        //public ICollection<PaymentAssignment> Payments { get; set; }
        //public ICollection<MailToRegistration> Mails { get; set; }
        public int ReminderLevel { get; set; }

        public string RespondentEmail { get; set; }
        public string RespondentFirstName { get; set; }
        public string RespondentLastName { get; set; }
        public ICollection<Seat> Seats_AsFollower { get; set; }
        public ICollection<Seat> Seats_AsLeader { get; set; }
        public string SoldOutMessage { get; set; }
        public RegistrationState State { get; set; }
        //public ICollection<Sms.Sms> Sms { get; set; }

        //public ICollection<Response> Responses { get; set; }
    }
}
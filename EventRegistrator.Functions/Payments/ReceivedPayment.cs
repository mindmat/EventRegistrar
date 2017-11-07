﻿using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Payments
{
    public class ReceivedPayment : Entity
    {
        public Guid? EventId { get; set; }
        public Guid? RegistrationId_Payer { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime BookingDate { get; set; }
        public string Info { get; set; }
        public string RecognizedEmail { get; set; }
        public string Reference { get; set; }
    }
}
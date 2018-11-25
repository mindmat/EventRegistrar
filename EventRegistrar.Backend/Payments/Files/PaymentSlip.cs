﻿using System;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Payments.Files
{
    public class PaymentSlip : Entity
    {
        public string ContentType { get; set; }
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public byte[] FileBinary { get; set; }
        public string Filename { get; set; }
        public ReceivedPayment ReceivedPayment { get; set; }
        public Guid? ReceivedPaymentId { get; set; }
        public string Reference { get; set; }
    }
}
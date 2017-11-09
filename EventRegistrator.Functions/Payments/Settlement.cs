using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Payments
{
    public class Settlement : Entity
    {
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
        public decimal Amount { get; set; }
    }
}
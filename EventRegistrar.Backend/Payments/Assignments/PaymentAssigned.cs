using System;
using System.Linq;

using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PaymentAssigned : DomainEvent
    {
        public Guid PaymentId { get; set; }
        public Guid? PaymentId_Counter { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? PayoutRequestId { get; set; }
        public Guid PaymentAssignmentId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentAssignedUserTranslation : IEventToUserTranslation<PaymentAssigned>
    {
        private readonly IQueryable<ReceivedPayment> _payments;
        private readonly IQueryable<Registration> _registrations;

        public PaymentAssignedUserTranslation(IQueryable<ReceivedPayment> payments,
                                              IQueryable<Registration> registrations)
        {
            _payments = payments;
            _registrations = registrations;
        }

        public string GetText(PaymentAssigned domainEvent)
        {
            var payment = _payments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentId);
            var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
            if (domainEvent.PaymentId_Counter != null)
            {
                var paymentCounter = _payments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentId_Counter);
                return $"Rückzahlung über {domainEvent.Amount} an {paymentCounter?.CreditorName} zu Zahlung über {payment?.Amount} von {payment?.DebitorName} zugeordnet (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
            }
            if (domainEvent.PayoutRequestId != null)
            {
                return $"Rückerstattung über {domainEvent.Amount} an {payment?.CreditorName} zugeordnet. Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
            }
            return $"Zahlungseingang über {domainEvent.Amount} von {payment?.DebitorName} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
        }
    }

}
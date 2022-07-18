using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class OutgoingPaymentAssigned : DomainEvent
{
    public Guid OutgoingPaymentId { get; set; }
    public Guid? PaymentId_Counter { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? PayoutRequestId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
    public decimal Amount { get; set; }
}

public class OutgoingPaymentAssignedUserTranslation : IEventToUserTranslation<OutgoingPaymentAssigned>
{
    private readonly IQueryable<OutgoingPayment> _outgoingPayments;
    private readonly IQueryable<Registration> _registrations;

    public OutgoingPaymentAssignedUserTranslation(IQueryable<OutgoingPayment> outgoingPayments,
                                                  IQueryable<Registration> registrations)
    {
        _outgoingPayments = outgoingPayments;
        _registrations = registrations;
    }

    public string GetText(OutgoingPaymentAssigned domainEvent)
    {
        var outgoingPayment = _outgoingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.OutgoingPaymentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        //if (domainEvent.PaymentId_Counter != null)
        //{
        //    var paymentCounter = _outgoingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentId_Counter);
        //    return
        //        $"Rückzahlung über {domainEvent.Amount} an {paymentCounter?.CreditorName} zu Zahlung über {outgoingPayment?.Amount} von {outgoingPayment?.DebitorName} zugeordnet (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
        //}

        if (domainEvent.PayoutRequestId != null)
        {
            return
                $"Rückerstattung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zugeordnet. Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
        }

        return $"Auszahlung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
    }
}
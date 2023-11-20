using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class RepaymentAssigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
    public Guid IncomingPaymentId { get; set; }
    public decimal Amount { get; set; }
}

public class RepaymentAssignedUserTranslation(IQueryable<OutgoingPayment> outgoingPayments,
                                              IQueryable<IncomingPayment> incomingPayments,
                                              IQueryable<Registration> registrations)
    : IEventToUserTranslation<RepaymentAssigned>
{
    public string GetText(RepaymentAssigned domainEvent)
    {
        var outgoingPayment = outgoingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.OutgoingPaymentId);
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        var incomingPayment = incomingPayments.Where(pmt => pmt.Id == domainEvent.IncomingPaymentId)
                                              .Include(pmt => pmt.Payment)
                                              .FirstOrDefault();
        return
            $"Rückzahlung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zu Zahlung über {incomingPayment?.Payment?.Amount} von {incomingPayment?.DebitorName} zugeordnet (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
    }
}
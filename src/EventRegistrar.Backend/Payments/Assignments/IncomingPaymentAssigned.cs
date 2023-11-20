using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class IncomingPaymentAssigned : DomainEvent
{
    public Guid IncomingPaymentId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
    public decimal Amount { get; set; }
}

public class IncomingPaymentAssignedUserTranslation(IQueryable<IncomingPayment> incomingPayments,
                                                    IQueryable<Registration> registrations)
    : IEventToUserTranslation<IncomingPaymentAssigned>
{
    public string GetText(IncomingPaymentAssigned domainEvent)
    {
        var incomingPaymentIncoming = incomingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.IncomingPaymentId);
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);

        return $"Zahlungseingang über {domainEvent.Amount} von {incomingPaymentIncoming?.DebitorName} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
    }
}
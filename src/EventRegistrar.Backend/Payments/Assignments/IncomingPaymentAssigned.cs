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

public class IncomingPaymentAssignedUserTranslation : IEventToUserTranslation<IncomingPaymentAssigned>
{
    private readonly IQueryable<IncomingPayment> _incomingPayments;
    private readonly IQueryable<Registration> _registrations;

    public IncomingPaymentAssignedUserTranslation(IQueryable<IncomingPayment> incomingPayments,
                                                  IQueryable<Registration> registrations)
    {
        _incomingPayments = incomingPayments;
        _registrations = registrations;
    }

    public string GetText(IncomingPaymentAssigned domainEvent)
    {
        var incomingPaymentIncoming = _incomingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.IncomingPaymentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);

        return $"Zahlungseingang über {domainEvent.Amount} von {incomingPaymentIncoming?.DebitorName} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
    }
}
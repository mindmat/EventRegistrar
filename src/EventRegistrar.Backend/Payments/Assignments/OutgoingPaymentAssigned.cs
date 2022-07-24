using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class OutgoingPaymentAssigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? PayoutRequestId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
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

        return domainEvent.PayoutRequestId != null
                   ? $"Rückerstattung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zugeordnet. Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet"
                   : $"Auszahlung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
    }
}
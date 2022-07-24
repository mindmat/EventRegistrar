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

public class RepaymentAssignedUserTranslation : IEventToUserTranslation<RepaymentAssigned>
{
    private readonly IQueryable<OutgoingPayment> _outgoingPayments;
    private readonly IQueryable<IncomingPayment> _incomingPayments;
    private readonly IQueryable<Registration> _registrations;

    public RepaymentAssignedUserTranslation(IQueryable<OutgoingPayment> outgoingPayments,
                                            IQueryable<IncomingPayment> incomingPayments,
                                            IQueryable<Registration> registrations)
    {
        _outgoingPayments = outgoingPayments;
        _incomingPayments = incomingPayments;
        _registrations = registrations;
    }

    public string GetText(RepaymentAssigned domainEvent)
    {
        var outgoingPayment = _outgoingPayments.FirstOrDefault(pmt => pmt.Id == domainEvent.OutgoingPaymentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        var incomingPayment = _incomingPayments.Where(pmt => pmt.Id == domainEvent.IncomingPaymentId)
                                               .Include(pmt => pmt.Payment)
                                               .FirstOrDefault();
        return
            $"Rückzahlung über {domainEvent.Amount} an {outgoingPayment?.CreditorName} zu Zahlung über {incomingPayment?.Payment?.Amount} von {incomingPayment?.DebitorName} zugeordnet (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
    }
}
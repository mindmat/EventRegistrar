using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PaymentUnassigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PaymentAssignmentId_Counter { get; set; }
    public Guid PaymentId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class PaymentUnassignedUserTranslation : IEventToUserTranslation<PaymentUnassigned>
{
    private readonly IQueryable<PaymentAssignment> _assignments;
    private readonly IQueryable<Registration> _registrations;

    public PaymentUnassignedUserTranslation(IQueryable<Registration> registrations,
                                            IQueryable<PaymentAssignment> assignments)
    {
        _registrations = registrations;
        _assignments = assignments;
    }

    public string GetText(PaymentUnassigned domainEvent)
    {
        var assignment = _assignments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentAssignmentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        //if (domainEvent.PaymentId_Counter != null)
        //{
        //    var paymentCounter = _payments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentId_Counter);
        //    return $"Rückzahlung über {domainEvent.Amount} an {paymentCounter?.CreditorName} zu Zahlung über {payment?.Amount} von {payment?.DebitorName} zugeordnet (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
        //}
        //if (domainEvent.PayoutRequestId != null)
        //{
        //    return $"Rückerstattung über {domainEvent.Amount} an {payment?.CreditorName} zugeordnet. Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} zugeordnet";
        //}
        return
            $"Zuordnung von Zahlung über {assignment?.Amount} von Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} rückgängig gemacht";
    }
}
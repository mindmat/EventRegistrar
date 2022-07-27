using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class OutgoingPaymentUnassigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PaymentAssignmentId_Counter { get; set; }
    public Guid OutgoingPaymentId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class OutgoingPaymentUnassignedUserTranslation : IEventToUserTranslation<OutgoingPaymentUnassigned>
{
    private readonly IQueryable<PaymentAssignment> _assignments;
    private readonly IQueryable<Registration> _registrations;

    public OutgoingPaymentUnassignedUserTranslation(IQueryable<Registration> registrations,
                                                    IQueryable<PaymentAssignment> assignments)
    {
        _registrations = registrations;
        _assignments = assignments;
    }

    public string GetText(OutgoingPaymentUnassigned domainEvent)
    {
        var assignment = _assignments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentAssignmentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);

        return $"Zuordnung von Auszahlung über {assignment?.Amount} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} rückgängig gemacht";
    }
}
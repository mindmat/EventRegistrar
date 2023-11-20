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

public class OutgoingPaymentUnassignedUserTranslation(IQueryable<Registration> registrations,
                                                      IQueryable<PaymentAssignment> assignments)
    : IEventToUserTranslation<OutgoingPaymentUnassigned>
{
    public string GetText(OutgoingPaymentUnassigned domainEvent)
    {
        var assignment = assignments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentAssignmentId);
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);

        return $"Zuordnung von Auszahlung über {assignment?.Amount} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} rückgängig gemacht";
    }
}
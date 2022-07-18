using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments;

public class IncomingPaymentUnassigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PaymentAssignmentId_Counter { get; set; }
    public Guid IncomingPaymentId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class IncomingPaymentUnassignedUserTranslation : IEventToUserTranslation<IncomingPaymentUnassigned>
{
    private readonly IQueryable<PaymentAssignment> _assignments;
    private readonly IQueryable<Registration> _registrations;

    public IncomingPaymentUnassignedUserTranslation(IQueryable<Registration> registrations,
                                                    IQueryable<PaymentAssignment> assignments)
    {
        _registrations = registrations;
        _assignments = assignments;
    }

    public string GetText(IncomingPaymentUnassigned domainEvent)
    {
        var assignment = _assignments.FirstOrDefault(pmt => pmt.Id == domainEvent.PaymentAssignmentId);
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);

        return $"Zuordnung von Zahlung über {assignment?.Amount} zu Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName} rückgängig gemacht";
    }
}
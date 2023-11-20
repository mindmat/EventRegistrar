using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Registrations.Confirmation;

public class SingleRegistrationPaid : DomainEvent
{
    public Guid RegistrationId { get; set; }
    public bool WillPayAtCheckin { get; set; }
}

public class SingleRegistrationPaidUserTranslation(IQueryable<Payment> payments,
                                                   IQueryable<Registration> registrations)
    : IEventToUserTranslation<SingleRegistrationPaid>
{
    private readonly IQueryable<Payment> _payments = payments;

    public string GetText(SingleRegistrationPaid domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat bezahlt";
    }
}
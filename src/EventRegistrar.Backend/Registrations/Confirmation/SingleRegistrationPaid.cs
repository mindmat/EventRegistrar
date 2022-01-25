using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Registrations.Confirmation;

public class SingleRegistrationPaid : DomainEvent
{
    public Guid RegistrationId { get; set; }
    public bool WillPayAtCheckin { get; set; }
}

public class SingleRegistrationPaidUserTranslation : IEventToUserTranslation<SingleRegistrationPaid>
{
    private readonly IQueryable<BankAccountBooking> _payments;
    private readonly IQueryable<Registration> _registrations;

    public SingleRegistrationPaidUserTranslation(IQueryable<BankAccountBooking> payments,
                                                 IQueryable<Registration> registrations)
    {
        _payments = payments;
        _registrations = registrations;
    }

    public string GetText(SingleRegistrationPaid domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat bezahlt";
    }
}
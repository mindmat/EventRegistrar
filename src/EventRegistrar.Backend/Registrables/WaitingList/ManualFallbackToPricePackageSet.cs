using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class ManualFallbackToPricePackageSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
}

public class ManualFallbackToPricePackageSetUserTranslation : IEventToUserTranslation<ManualFallbackToPricePackageSet>
{
    private readonly IQueryable<Registration> _registrations;

    public ManualFallbackToPricePackageSetUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(ManualFallbackToPricePackageSet domainEvent)
    {
        var registration = _registrations.Where(reg => reg.Id == domainEvent.RegistrationId)
                                         .Select(reg => new
                                                        {
                                                            reg.RespondentFirstName,
                                                            reg.RespondentLastName,
                                                            FallbacPricePackageName = reg.PricePackage_ManualFallback!.Name
                                                        })
                                         .FirstOrDefault();
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wünscht {registration?.FallbacPricePackageName} als Fallback";
    }
}

public class RecalculatePriceWhenFallbackSet : IEventToCommandTranslation<ManualFallbackToPricePackageSet>
{
    public IEnumerable<IRequest> Translate(ManualFallbackToPricePackageSet e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}
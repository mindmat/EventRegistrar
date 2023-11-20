using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class ManualFallbackToPricePackageSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
}

public class ManualFallbackToPricePackageSetUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<ManualFallbackToPricePackageSet>
{
    public string GetText(ManualFallbackToPricePackageSet domainEvent)
    {
        var registration = registrations.Where(reg => reg.Id == domainEvent.RegistrationId)
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
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class ManualFallbackToPricePackageSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
}

public class ManualFallbackToPricePackageSetUserTranslation(IQueryable<Registration> registrations,
                                                            IQueryable<Registrable> registrables)
    : IEventToUserTranslation<ManualFallbackToPricePackageSet>
{
    public string GetText(ManualFallbackToPricePackageSet domainEvent)
    {
        var registration = registrations.Where(reg => reg.Id == domainEvent.RegistrationId)
                                        .Select(reg => new
                                                       {
                                                           reg.RespondentFirstName,
                                                           reg.RespondentLastName,
                                                           reg.PricePackageIds_ManualFallback
                                                       })
                                        .FirstOrDefault();
        List<string>? trackNames = null;
        if (registration?.PricePackageIds_ManualFallback?.Any() == true)
        {
            trackNames = registrables.Where(rbl => registration.PricePackageIds_ManualFallback.Contains(rbl.Id))
                                     .Select(rbl => rbl.Name)
                                     .ToList();
        }

        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wünscht {trackNames?.StringJoin()} als Fallback";
    }
}

public class RecalculatePriceWhenFallbackSet : IEventToCommandTranslation<ManualFallbackToPricePackageSet>
{
    public IEnumerable<IRequest> Translate(ManualFallbackToPricePackageSet e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}
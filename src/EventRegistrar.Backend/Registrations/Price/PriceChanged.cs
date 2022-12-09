using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Price;

public class PriceChanged : DomainEvent
{
    public decimal NewPrice { get; set; }
    public decimal OldPrice { get; set; }
    public Guid RegistrationId { get; set; }
}

public class PriceChangedUserTranslation : IEventToUserTranslation<PriceChanged>
{
    public string GetText(PriceChanged domainEvent)
    {
        return $"Preis bisher: {domainEvent.OldPrice:##.00}, Preis neu {domainEvent.NewPrice:##.00}";
    }
}
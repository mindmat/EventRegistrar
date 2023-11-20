using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class AutomaticPromotionActivated : DomainEvent
{
    public Guid RegistrableId { get; set; }
}

public class AutomaticPromotionActivatedUserTranslation(IQueryable<Registrable> registrables) : IEventToUserTranslation<AutomaticPromotionActivated>
{
    public string GetText(AutomaticPromotionActivated domainEvent)
    {
        var registrable = registrables.FirstOrDefault(reg => reg.Id == domainEvent.RegistrableId);
        return $"Automatisches Nachrücken für {registrable?.DisplayName} aktiviert";
    }
}
using System;
using System.Linq;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList.Promotion
{
    public class AutomaticPromotionActivated : DomainEvent
    {
        public Guid RegistrableId { get; set; }
    }

    public class AutomaticPromotionActivatedUserTranslation : IEventToUserTranslation<AutomaticPromotionActivated>
    {
        private readonly IQueryable<Registrable> _registrables;

        public AutomaticPromotionActivatedUserTranslation(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public string GetText(AutomaticPromotionActivated domainEvent)
        {
            var registrable = _registrables.FirstOrDefault(reg => reg.Id == domainEvent.RegistrableId);
            return $"Automatisches Nachrücken für {registrable?.Name} aktiviert";
        }
    }
}
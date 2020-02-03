using System;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Spots
{
    public class SpotAdded : DomainEvent
    {
        public bool IsInitialProcessing { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
        public string Registrable { get; set; }
        public string Participant { get; set; }
    }

    public class SpotAddedUserTranslation : IEventToUserTranslation<SpotAdded>
    {
        public string GetText(SpotAdded domainEvent)
        {
            return $"für {domainEvent.Registrable}";
        }
    }
}
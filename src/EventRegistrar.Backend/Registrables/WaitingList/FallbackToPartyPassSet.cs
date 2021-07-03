using System;
using System.Linq;

using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class FallbackToPartyPassSet : DomainEvent
    {
        public Guid RegistrationId { get; set; }
    }

    public class FallbackToPartyPassSetUserTranslation : IEventToUserTranslation<FallbackToPartyPassSet>
    {
        private readonly IQueryable<Registration> _registrations;

        public FallbackToPartyPassSetUserTranslation(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public string GetText(FallbackToPartyPassSet domainEvent)
        {
            var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
            return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wünscht einen Partypass falls nicht im Kurs";
        }
    }
}
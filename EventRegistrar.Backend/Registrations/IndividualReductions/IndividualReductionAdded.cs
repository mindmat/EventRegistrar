using System;
using System.Linq;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.IndividualReductions
{
    public class IndividualReductionAdded : DomainEvent
    {
        public decimal Amount { get; set; }
        public Guid RegistrationId { get; set; }
        public string Reason { get; set; }
    }

    public class IndividualReductionAddedUserTranslation : IEventToUserTranslation<IndividualReductionAdded>
    {
        private readonly IQueryable<Registration> _registrations;

        public IndividualReductionAddedUserTranslation(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public string GetText(IndividualReductionAdded domainEvent)
        {
            var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
            return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wurde ein persönlicher Rabatt über {domainEvent.Amount} gewährt. Begründung: {domainEvent.Reason}";
        }
    }
}
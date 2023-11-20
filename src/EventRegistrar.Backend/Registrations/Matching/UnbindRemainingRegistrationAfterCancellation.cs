using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindRemainingRegistrationAfterCancellation(IQueryable<Registration> registrations) : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        var partnerRegistrationId = registrations.Where(reg => reg.Id == e.RegistrationId)
                                                 .Select(reg => reg.RegistrationId_Partner)
                                                 .FirstOrDefault();
        if (partnerRegistrationId != null && e.EventId != null)
        {
            yield return new UnbindPartnerRegistrationCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = partnerRegistrationId.Value
                         };
        }
    }
}
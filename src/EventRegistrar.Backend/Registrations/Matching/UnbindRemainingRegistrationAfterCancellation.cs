using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Cancel;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindRemainingRegistrationAfterCancellation : IEventToCommandTranslation<RegistrationCancelled>
{
    private readonly IQueryable<Registration> _registrations;

    public UnbindRemainingRegistrationAfterCancellation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        var partnerRegistrationId = _registrations.Where(reg => reg.Id == e.RegistrationId)
                                                  .Select(reg => reg.RegistrationId_Partner)
                                                  .FirstOrDefault();
        if (partnerRegistrationId != null && e.EventId != null)
            yield return new UnbindPartnerRegistrationCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = partnerRegistrationId.Value
                         };
    }
}
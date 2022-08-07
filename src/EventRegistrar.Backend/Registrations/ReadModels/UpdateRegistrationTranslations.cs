using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;

using MediatR;

namespace EventRegistrar.Backend.Registrations.ReadModels;

public class UpdateRegistrationWhenOutgoingPaymentAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenOutgoingPaymentUnassigned : IEventToCommandTranslation<OutgoingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenIncomingPaymentUnassigned : IEventToCommandTranslation<IncomingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenIncomingPaymentAssigned : IEventToCommandTranslation<IncomingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}
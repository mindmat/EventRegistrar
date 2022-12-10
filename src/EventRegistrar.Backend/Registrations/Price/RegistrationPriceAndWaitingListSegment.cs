using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Registrations.Price;

public class RegistrationPriceAndWaitingListSegment : IDirtySegment
{
    public void EnqueueCommand(CommandQueue commandQueue, Guid entityId)
    {
        commandQueue.EnqueueCommand(new RecalculatePriceAndWaitingListCommand { RegistrationId = entityId });
    }

    public string Entity => nameof(Registration);
    public string Name => nameof(RegistrationPriceAndWaitingListSegment);
}
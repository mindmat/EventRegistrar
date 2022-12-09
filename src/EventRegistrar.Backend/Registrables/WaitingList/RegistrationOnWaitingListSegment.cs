using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class RegistrationOnWaitingListSegment : IDirtySegment
{
    public void EnqueueCommand(CommandQueue commandQueue, Guid entityId)
    {
        commandQueue.EnqueueCommand(new CheckIfRegistrationHasMovedUpCommand { RegistrationId = entityId });
    }

    public string Entity => nameof(Registration);
    public string Name => nameof(RegistrationOnWaitingListSegment);
}
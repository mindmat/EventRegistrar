using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculateAllPriceAndWaitingListCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RecalculateAllPriceAndWaitingListCommandHandler(IQueryable<Registration> registrations,
                                                             CommandQueue commandQueue)
    : IRequestHandler<RecalculateAllPriceAndWaitingListCommand>
{
    public async Task Handle(RecalculateAllPriceAndWaitingListCommand command, CancellationToken cancellationToken)
    {
        var registrationIds = await registrations.Where(reg => reg.EventId == command.EventId
                                                            && reg.State != RegistrationState.Cancelled)
                                                 .Select(reg => reg.Id)
                                                 .ToListAsync(cancellationToken);
        foreach (var registrationId in registrationIds)
        {
            commandQueue.EnqueueCommand(new RecalculatePriceAndWaitingListCommand
                                        {
                                            RegistrationId = registrationId
                                        });
        }
    }
}
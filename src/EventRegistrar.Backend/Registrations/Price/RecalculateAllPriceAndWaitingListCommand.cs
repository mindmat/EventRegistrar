using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculateAllPriceAndWaitingListCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RecalculateAllPriceAndWaitingListCommandHandler : AsyncRequestHandler<RecalculateAllPriceAndWaitingListCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly CommandQueue _commandQueue;

    public RecalculateAllPriceAndWaitingListCommandHandler(IQueryable<Registration> registrations,
                                                           CommandQueue commandQueue)
    {
        _registrations = registrations;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(RecalculateAllPriceAndWaitingListCommand command, CancellationToken cancellationToken)
    {
        var registrationIds = await _registrations.Where(reg => reg.EventId == command.EventId
                                                             && reg.State != RegistrationState.Cancelled)
                                                  .Select(reg => reg.Id)
                                                  .ToListAsync(cancellationToken);
        foreach (var registrationId in registrationIds)
        {
            _commandQueue.EnqueueCommand(new RecalculatePriceAndWaitingListCommand
                                         {
                                             RegistrationId = registrationId
                                         });
        }
    }
}
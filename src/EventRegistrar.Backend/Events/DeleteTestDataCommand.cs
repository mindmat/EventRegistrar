namespace EventRegistrar.Backend.Events;

public class DeleteTestDataCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
}

public class DeleteTestDataCommandHandler : AsyncRequestHandler<DeleteTestDataCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly TestDataDeleter _testDataDeleter;

    public DeleteTestDataCommandHandler(IQueryable<Event> events,
                                        TestDataDeleter testDataDeleter)
    {
        _events = events;
        _testDataDeleter = testDataDeleter;
    }

    protected override async Task Handle(DeleteTestDataCommand command, CancellationToken cancellationToken)
    {
        var eventToOpen = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        await _testDataDeleter.DeleteTestData(eventToOpen, cancellationToken);
    }
}
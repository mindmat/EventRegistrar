namespace EventRegistrar.Backend.Events;

public class DeleteTestDataCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
}

public class DeleteTestDataCommandHandler(IQueryable<Event> events,
                                          TestDataDeleter testDataDeleter)
    : IRequestHandler<DeleteTestDataCommand>
{
    public async Task Handle(DeleteTestDataCommand command, CancellationToken cancellationToken)
    {
        var eventToOpen = await events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        await testDataDeleter.DeleteTestData(eventToOpen, cancellationToken);
    }
}
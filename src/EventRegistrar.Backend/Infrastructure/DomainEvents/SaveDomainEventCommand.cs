namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class SaveDomainEventCommand : IRequest
{
    public Guid DomainEventId { get; set; }
    public Guid? DomainEventId_Parent { get; set; }
    public string EventData { get; set; }
    public Guid? EventId { get; set; }
    public string EventType { get; set; }
}

public class SaveDomainEventCommandHandler(DbContext dbContext,
                                           IDateTimeProvider dateTimeProvider) : IRequestHandler<SaveDomainEventCommand>
{
    private readonly DbSet<PersistedDomainEvent> _domainEvents = dbContext.Set<PersistedDomainEvent>();

    public async Task Handle(SaveDomainEventCommand command, CancellationToken cancellationToken)
    {
        var id = command.DomainEventId == Guid.Empty ? Guid.NewGuid() : command.DomainEventId;
        await _domainEvents.AddAsync(new PersistedDomainEvent
                                     {
                                         Id = id,
                                         EventId = command.EventId,
                                         DomainEventId_Parent = command.DomainEventId_Parent,
                                         Timestamp = dateTimeProvider.Now,
                                         Type = command.EventType,
                                         Data = command.EventData
                                     }, cancellationToken);
    }
}
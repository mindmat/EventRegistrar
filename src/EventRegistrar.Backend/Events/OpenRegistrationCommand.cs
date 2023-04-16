using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations.Overview;

namespace EventRegistrar.Backend.Events;

public class OpenRegistrationCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public bool DeleteTestData { get; set; }
}

public class OpenRegistrationCommandHandler : AsyncRequestHandler<OpenRegistrationCommand>
{
    private readonly IRepository<Event> _events;
    private readonly TestDataDeleter _testDataDeleter;
    private readonly IEventBus _eventBus;

    public OpenRegistrationCommandHandler(IRepository<Event> events,
                                          TestDataDeleter testDataDeleter,
                                          IEventBus eventBus)
    {
        _events = events;
        _testDataDeleter = testDataDeleter;
        _eventBus = eventBus;
    }

    protected override async Task Handle(OpenRegistrationCommand command, CancellationToken cancellationToken)
    {
        var eventToOpen = await _events.AsTracking()
                                       .FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (eventToOpen.State != EventState.Setup)
        {
            throw new ArgumentException($"Event {eventToOpen.Id} is in state {eventToOpen.State} and can therefore not be opened");
        }

        if (command.DeleteTestData)
        {
            await _testDataDeleter.DeleteTestData(eventToOpen, cancellationToken);
        }

        eventToOpen.State = EventState.RegistrationOpen;
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(EventQuery)
                          });
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(EventByAcronymQuery)
                          });
    }
}
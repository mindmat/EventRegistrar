using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events;

public class OpenRegistrationCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public bool DeleteTestData { get; set; }
}

public class OpenRegistrationCommandHandler(IRepository<Event> events,
                                            TestDataDeleter testDataDeleter,
                                            IEventBus eventBus)
    : IRequestHandler<OpenRegistrationCommand>
{
    public async Task Handle(OpenRegistrationCommand command, CancellationToken cancellationToken)
    {
        var eventToOpen = await events.AsTracking()
                                      .FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (eventToOpen.State != EventState.Setup)
        {
            throw new ArgumentException($"Event {eventToOpen.Id} is in state {eventToOpen.State} and can therefore not be opened");
        }

        if (command.DeleteTestData)
        {
            await testDataDeleter.DeleteTestData(eventToOpen, cancellationToken);
        }

        eventToOpen.State = EventState.RegistrationOpen;
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(EventQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(EventByAcronymQuery)
                         });
    }
}
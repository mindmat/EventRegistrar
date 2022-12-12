using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Raw;

namespace EventRegistrar.Backend.Registrations.Register;

public class StartProcessAllPendingRawRegistrationsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class StartProcessAllPendingRawRegistrationsCommandHandler : AsyncRequestHandler<StartProcessAllPendingRawRegistrationsCommand>
{
    private readonly IQueryable<RawRegistration> _rawRegistrations;
    private readonly IQueryable<Event> _events;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CommandQueue _commandQueue;

    public StartProcessAllPendingRawRegistrationsCommandHandler(IQueryable<RawRegistration> rawRegistrations,
                                                                IQueryable<Event> events,
                                                                IDateTimeProvider dateTimeProvider,
                                                                CommandQueue commandQueue)
    {
        _rawRegistrations = rawRegistrations;
        _events = events;
        _dateTimeProvider = dateTimeProvider;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(StartProcessAllPendingRawRegistrationsCommand request, CancellationToken cancellationToken)
    {
        var eventAcronym = await _events.Where(evt => evt.Id == request.EventId)
                                        .Select(evt => evt.Acronym)
                                        .FirstAsync(cancellationToken);
        var createdThreshold = _dateTimeProvider.Now.AddMinutes(-5); // try to avoid that a user is faster than the processing pipeline
        var ids = await _rawRegistrations.Where(rrg => rrg.EventAcronym == eventAcronym
                                                    && rrg.Processed == null
                                                    && rrg.Created < createdThreshold)
                                         .Select(rrg => rrg.Id)
                                         .ToListAsync(cancellationToken);
        foreach (var id in ids)
        {
            _commandQueue.EnqueueCommand(new ProcessRawRegistrationCommand { RawRegistrationId = id });
        }
    }
}
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateAllReadModelsOfEventCommand : IRequest
{
    public Guid EventId { get; set; }
}

public class StartUpdateAllReadModelsOfEventCommandHandler : IRequestHandler<StartUpdateAllReadModelsOfEventCommand>
{
    private readonly CommandQueue _commandQueue;
    private readonly IQueryable<Registration> _registrations;

    public StartUpdateAllReadModelsOfEventCommandHandler(CommandQueue commandQueue,
                                                         IQueryable<Registration> registrations)
    {
        _commandQueue = commandQueue;
        _registrations = registrations;
    }

    public async Task<Unit> Handle(StartUpdateAllReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        var registrationIds = await _registrations.Where(reg => reg.EventId == command.EventId)
                                                  .Select(reg => reg.Id)
                                                  .ToListAsync(cancellationToken);

        foreach (var registrationId in registrationIds)
        {
            _commandQueue.EnqueueCommand(new UpdateRegistrationQueryReadModelCommand
                                         {
                                             EventId = command.EventId,
                                             RegistrationId = registrationId
                                         });
        }

        return Unit.Value;
    }
}
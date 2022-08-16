using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class CreateRegistrableParameters
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDoubleRegistrable { get; set; }
}

public class CreateRegistrableCommand : IRequest, IEventBoundRequest
{
    public CreateRegistrableParameters Parameters { get; set; }

    public Guid EventId { get; set; }
}

public class CreateRegistrableCommandHandler : IRequestHandler<CreateRegistrableCommand>
{
    private readonly IRepository<Registrable> _registrables;
    private readonly CommandQueue _commandQueue;
    private IDateTimeProvider _dateTimeProvider;

    public CreateRegistrableCommandHandler(IRepository<Registrable> registrables,
                                           CommandQueue commandQueue,
                                           IDateTimeProvider dateTimeProvider)
    {
        _registrables = registrables;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(CreateRegistrableCommand command, CancellationToken cancellationToken)
    {
        var registrable = new Registrable
                          {
                              Id = command.Parameters.Id,
                              EventId = command.EventId,
                              Name = command.Parameters.Name
                          };
        await _registrables.InsertOrUpdateEntity(registrable, cancellationToken);
        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = nameof(RegistrablesOverviewQuery),
                                         EventId = command.EventId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });
        return Unit.Value;
    }
}
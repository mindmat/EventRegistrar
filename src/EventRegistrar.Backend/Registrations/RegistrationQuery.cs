using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationQuery : IRequest<SerializedJson<RegistrationDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RegistrationQueryHandler : IRequestHandler<RegistrationQuery, SerializedJson<RegistrationDisplayItem>>
{
    private readonly ReadModelReader _readModelReader;

    public RegistrationQueryHandler(ReadModelReader readModelReader)
    {
        _readModelReader = readModelReader;
    }


    public async Task<SerializedJson<RegistrationDisplayItem>> Handle(RegistrationQuery query, CancellationToken cancellationToken)
    {
        return await _readModelReader.Get<RegistrationDisplayItem>(nameof(RegistrationQuery),
                                                                   query.EventId,
                                                                   query.RegistrationId,
                                                                   cancellationToken);
    }
}
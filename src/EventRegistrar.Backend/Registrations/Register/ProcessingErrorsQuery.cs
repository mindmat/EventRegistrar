using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Registrations.Register;

public class ProcessingErrorsQuery : IRequest<SerializedJson<IEnumerable<ProcessingError>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ProcessingErrorsQueryHandler(ReadModelReader readModelReader) : IRequestHandler<ProcessingErrorsQuery, SerializedJson<IEnumerable<ProcessingError>>>
{
    public async Task<SerializedJson<IEnumerable<ProcessingError>>> Handle(ProcessingErrorsQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<ProcessingError>>(nameof(ProcessingErrorsQuery),
                                                                       query.EventId,
                                                                       null, 
                                                                       cancellationToken);
    }
}
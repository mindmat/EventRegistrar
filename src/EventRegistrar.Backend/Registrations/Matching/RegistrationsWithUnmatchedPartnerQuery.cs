using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Registrations.Matching;

public class RegistrationsWithUnmatchedPartnerQuery : IEventBoundRequest, IRequest<SerializedJson<IEnumerable<PotentialPartnerMatch>>>
{
    public Guid EventId { get; set; }
}

public class RegistrationsWithUnmatchedPartnerQueryHandler(ReadModelReader readModelReader) 
    : IRequestHandler<RegistrationsWithUnmatchedPartnerQuery, SerializedJson<IEnumerable<PotentialPartnerMatch>>>
{
    public async Task<SerializedJson<IEnumerable<PotentialPartnerMatch>>> Handle(RegistrationsWithUnmatchedPartnerQuery query,
                                                                                CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<PotentialPartnerMatch>>(nameof(RegistrationsWithUnmatchedPartnerQuery),
                                                                             query.EventId,
                                                                             null,
                                                                             cancellationToken);

    }
}
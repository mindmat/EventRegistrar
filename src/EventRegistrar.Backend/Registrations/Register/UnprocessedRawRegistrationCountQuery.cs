using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations.Raw;

namespace EventRegistrar.Backend.Registrations.Register;

public class UnprocessedRawRegistrationCountQuery : IRequest<UnprocessedRawRegistrationsInfo>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class UnprocessedRawRegistrationCountQueryHandler : IRequestHandler<UnprocessedRawRegistrationCountQuery, UnprocessedRawRegistrationsInfo>
{
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<RawRegistration> _rawRegistrations;

    public UnprocessedRawRegistrationCountQueryHandler(IQueryable<Event> events, IQueryable<RawRegistration> rawRegistrations)
    {
        _events = events;
        _rawRegistrations = rawRegistrations;
    }

    public async Task<UnprocessedRawRegistrationsInfo> Handle(UnprocessedRawRegistrationCountQuery request, CancellationToken cancellationToken)
    {
        var eventAcronym = await _events.Where(evt => evt.Id == request.EventId)
                                        .Select(evt => evt.Acronym)
                                        .FirstAsync(cancellationToken);
        return await _rawRegistrations.Where(rrg => rrg.EventAcronym == eventAcronym)
                                      .GroupBy(rrg => rrg.EventAcronym)
                                      .Select(grp => new UnprocessedRawRegistrationsInfo(grp.Count(rrg => rrg.Processed == null),
                                                                                         grp.Where(rrg => rrg.Processed == null).Min(rrg => rrg.Created),
                                                                                         grp.Where(rrg => rrg.Processed == null).Max(rrg => rrg.Created),
                                                                                         grp.Where(rrg => rrg.Processed != null).Min(rrg => rrg.Processed),
                                                                                         grp.Where(rrg => rrg.Processed != null).Max(rrg => rrg.Processed),
                                                                                         grp.Where(rrg => rrg.Processed == null
                                                                                                       && rrg.LastProcessingError != null)
                                                                                            .Select(rrg => rrg.LastProcessingError!)))
                                      .FirstAsync(cancellationToken);
    }
}

public record struct UnprocessedRawRegistrationsInfo(int Count,
                                                     DateTimeOffset? FirstUnprocessed,
                                                     DateTimeOffset? LastUnprocessed,
                                                     DateTimeOffset? FirstProcessed,
                                                     DateTimeOffset? LastProcessed,
                                                     IEnumerable<string> ProcessingErrors);
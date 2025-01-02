namespace EventRegistrar.Backend.Registrations.Overview
{
    public class RegistrationsPerDayQuery : IRequest<IEnumerable<RegistrationsPerDay>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationsPerDayQueryHandler(IQueryable<Registration> registrations) : IRequestHandler<RegistrationsPerDayQuery, IEnumerable<RegistrationsPerDay>>
    {
        public async Task<IEnumerable<RegistrationsPerDay>> Handle(RegistrationsPerDayQuery query, CancellationToken cancellationToken)
        {
            var data = await registrations.Where(reg => reg.EventId == query.EventId)
                                          .Select(reg => new { reg.ReceivedAt, reg.State })
                                          .ToListAsync(cancellationToken);

            return data.GroupBy(reg => reg.ReceivedAt.Date)
                       .OrderBy(grp => grp.Key.Date)
                       .Select(grp => new RegistrationsPerDay(grp.Key,
                                                              grp.Count(reg => reg.State != RegistrationState.Cancelled),
                                                              grp.Count(reg => reg.State == RegistrationState.Cancelled)))
                       .ToList();
        }
    }

    public record RegistrationsPerDay(DateTime Date, int CountActive, int CountCancelled);
}
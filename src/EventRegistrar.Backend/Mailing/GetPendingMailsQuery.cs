using EventRegistrar.Backend.Authorization;

using MediatR;

namespace EventRegistrar.Backend.Mailing;

public class GetPendingMailsQuery : IRequest<IEnumerable<Mail>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class GetPendingMailsQueryHandler : IRequestHandler<GetPendingMailsQuery, IEnumerable<Mail>>
{
    private readonly IQueryable<Mail> _mails;

    public GetPendingMailsQueryHandler(IQueryable<Mail> mails)
    {
        _mails = mails;
    }

    public async Task<IEnumerable<Mail>> Handle(GetPendingMailsQuery query, CancellationToken cancellationToken)
    {
        var mails = await _mails
                          .Where(mail => (mail.Registrations.Any(map => map.Registration.EventId == query.EventId)
                                       || mail.EventId == query.EventId)
                                      && mail.Withhold
                                      && !mail.Discarded)
                          .OrderByDescending(mail => mail.Created)
                          .ToListAsync(cancellationToken);
        return mails;
    }
}
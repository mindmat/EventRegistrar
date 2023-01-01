namespace EventRegistrar.Backend.Mailing.Bulk;

public class GeneratedBulkMailsQuery : IRequest<GeneratedBulkMails>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? BulkMailKey { get; set; }
}

public class GeneratedBulkMailsQueryHandler : IRequestHandler<GeneratedBulkMailsQuery, GeneratedBulkMails>
{
    private readonly IQueryable<Mail> _mails;

    public GeneratedBulkMailsQueryHandler(IQueryable<Mail> mails)
    {
        _mails = mails;
    }

    public async Task<GeneratedBulkMails> Handle(GeneratedBulkMailsQuery query, CancellationToken cancellationToken)
    {
        if (query.BulkMailKey == null)
        {
            throw new ArgumentNullException(nameof(query.BulkMailKey));
        }

        return await _mails.Where(mail => mail.BulkMailKey == query.BulkMailKey)
                           .GroupBy(mail => mail.BulkMailKey)
                           .Select(grp =>
                                       new GeneratedBulkMails
                                       {
                                           GeneratedCount = grp.Count(),
                                           SentCount = grp.Count(mail => mail.Sent != null)
                                       })
                           .FirstAsync(cancellationToken);
    }
}

public class GeneratedBulkMails
{
    public int GeneratedCount { get; set; }
    public int SentCount { get; set; }
}
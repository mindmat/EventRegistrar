namespace EventRegistrar.Backend.Mailing.Bulk;

public class GeneratedBulkMailsQuery : IRequest<GeneratedBulkMails>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? BulkMailKey { get; set; }
}

public class GeneratedBulkMailsQueryHandler(IQueryable<Mail> mails) : IRequestHandler<GeneratedBulkMailsQuery, GeneratedBulkMails>
{
    public async Task<GeneratedBulkMails> Handle(GeneratedBulkMailsQuery query, CancellationToken cancellationToken)
    {
        if (query.BulkMailKey == null)
        {
            throw new ArgumentNullException(nameof(query.BulkMailKey));
        }

        return await mails.Where(mail => mail.BulkMailKey == query.BulkMailKey)
                          .GroupBy(mail => mail.BulkMailKey)
                          .Select(grp => new GeneratedBulkMails
                                         {
                                             GeneratedCount = grp.Count(),
                                             SentCount = grp.Count(mail => mail.Sent != null)
                                         })
                          .FirstOrDefaultAsync(cancellationToken)
            ?? new GeneratedBulkMails
               {
                   GeneratedCount = 0,
                   SentCount = 0
               };
    }
}

public class GeneratedBulkMails
{
    public int GeneratedCount { get; set; }
    public int SentCount { get; set; }
}
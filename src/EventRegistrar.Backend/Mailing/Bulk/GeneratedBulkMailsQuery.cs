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

        var data = await mails.Where(mail => mail.BulkMailKey == query.BulkMailKey
                                          && mail.EventId == query.EventId)
                              .GroupBy(mail => new { mail.Sent, mail.State })
                              .Select(grp => new
                                             {
                                                 grp.Key.Sent,
                                                 grp.Key.State,
                                                 Count = grp.Count()
                                             })
                              .ToListAsync(cancellationToken);
        return new GeneratedBulkMails
               {
                   Generated = data.Sum(grp => grp.Count),
                   Sent = data.Where(grp => grp.Sent != null)
                              .Sum(grp => grp.Count),
                   Processed = data.Where(grp => grp.State == MailState.Processed)
                                   .Sum(grp => grp.Count),
                   Dropped = data.Where(grp => grp.State == MailState.Dropped)
                                 .Sum(grp => grp.Count),
                   Delivered = data.Where(grp => grp.State == MailState.Delivered)
                                   .Sum(grp => grp.Count),
                   Bounce = data.Where(grp => grp.State == MailState.Bounce)
                                .Sum(grp => grp.Count),
                   Opened = data.Where(grp => grp.State is MailState.Open or MailState.Click)
                                .Sum(grp => grp.Count)
               };
    }
}

public class GeneratedBulkMails
{
    public int Generated { get; set; }
    public int Sent { get; set; }
    public int Processed { get; set; }
    public int Dropped { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Bounce { get; set; }
}
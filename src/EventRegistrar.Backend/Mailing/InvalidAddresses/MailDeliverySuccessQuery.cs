using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class MailDeliverySuccessQuery : IEventBoundRequest, IRequest<MailDeliverySuccess>
{
    public Guid EventId { get; set; }
    public int? SentSinceDays { get; set; }
}

public record MailDeliverySuccess
{
    public int Sent { get; set; }
    public int Processed { get; set; }
    public int Dropped { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Bounce { get; set; }
}

public class MailDeliverySuccessQueryHandler(IQueryable<Mail> mails, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<MailDeliverySuccessQuery, MailDeliverySuccess>
{
    public async Task<MailDeliverySuccess> Handle(MailDeliverySuccessQuery query, CancellationToken cancellationToken)
    {
        var sentSince = dateTimeProvider.Now.AddDays(-Math.Abs(query.SentSinceDays ?? 2));
        var data = await mails.Where(mtp => mtp.EventId == query.EventId
                                         && mtp.Sent >= sentSince)
                              .GroupBy(mail => new { mail.Sent, mail.State })
                              .Select(grp => new
                                             {
                                                 grp.Key.State,
                                                 Count = grp.Count()
                                             })
                              .ToListAsync(cancellationToken);

        return new MailDeliverySuccess
               {
                   Sent = data.Sum(grp => grp.Count),
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
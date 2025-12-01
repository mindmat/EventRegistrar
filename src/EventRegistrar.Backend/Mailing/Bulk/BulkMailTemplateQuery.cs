using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailTemplateQuery : IEventBoundRequest, IRequest<BulkMailTemplateDisplayItem>
{
    public Guid EventId { get; set; }
    public Guid BulkMailTemplateId { get; set; }
}

public class BulkMailTemplateQueryHandler(IQueryable<BulkMailTemplate> mailTemplates) : IRequestHandler<BulkMailTemplateQuery, BulkMailTemplateDisplayItem>
{
    public async Task<BulkMailTemplateDisplayItem> Handle(BulkMailTemplateQuery query, CancellationToken cancellationToken)
    {
        return await mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                             && mtp.Id == query.BulkMailTemplateId)
                                  .Select(mtp => new BulkMailTemplateDisplayItem
                                                 {
                                                     Id = mtp.Id,
                                                     BulkMailKey = mtp.BulkMailKey,
                                                     SenderMail = mtp.SenderMail,
                                                     SenderName = mtp.SenderName,
                                                     Subject = mtp.Subject,
                                                     ContentHtml = mtp.ContentHtml,
                                                     Audiences = mtp.MailingAudience.GetFlags(),
                                                     RegistrableIds = mtp.RegistrableIds,
                                                     Discarded = mtp.Discarded,
                                                     AddIcs = mtp.AddIcs
                                                 })
                                  .FirstAsync(cancellationToken);
    }
}

public class BulkMailTemplateDisplayItem
{
    public Guid Id { get; set; }
    public string BulkMailKey { get; set; } = null!;
    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
    public IEnumerable<MailingAudience>? Audiences { get; set; }
    public IEnumerable<Guid>? RegistrableIds { get; set; }
    public bool Discarded { get; set; }
    public bool AddIcs { get; set; }
}
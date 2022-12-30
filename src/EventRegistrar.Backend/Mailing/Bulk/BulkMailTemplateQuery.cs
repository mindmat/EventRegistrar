using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailTemplateQuery : IEventBoundRequest, IRequest<BulkMailTemplateDisplayItem>
{
    public Guid EventId { get; set; }
    public Guid BulkMailTemplateId { get; set; }
}

public class BulkMailTemplateQueryHandler : IRequestHandler<BulkMailTemplateQuery, BulkMailTemplateDisplayItem>
{
    private readonly IQueryable<BulkMailTemplate> _mailTemplates;

    public BulkMailTemplateQueryHandler(IQueryable<BulkMailTemplate> mailTemplates)
    {
        _mailTemplates = mailTemplates;
    }

    public async Task<BulkMailTemplateDisplayItem> Handle(BulkMailTemplateQuery query, CancellationToken cancellationToken)
    {
        return await _mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                              && mtp.Id == query.BulkMailTemplateId)
                                   .Select(mtp => new BulkMailTemplateDisplayItem
                                                  {
                                                      Id = mtp.Id,
                                                      Subject = mtp.Subject,
                                                      ContentHtml = mtp.ContentHtml,
                                                      Audiences = mtp.MailingAudience.GetFlags()
                                                  })
                                   .FirstAsync(cancellationToken);
    }
}

public class BulkMailTemplateDisplayItem
{
    public Guid Id { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
    public IEnumerable<MailingAudience>? Audiences { get; set; }
}
using EventRegistrar.Backend.Mailing.Compose;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailPreviewQuery : IEventBoundRequest, IRequest<BulkMailPreview>
{
    public Guid EventId { get; set; }
    public Guid BulkMailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class BulkMailPreview
{
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class BulkMailPreviewQueryHandler(IQueryable<BulkMailTemplate> mailTemplates,
                                         MailComposer mailComposer)
    : IRequestHandler<BulkMailPreviewQuery, BulkMailPreview>
{
    public async Task<BulkMailPreview> Handle(BulkMailPreviewQuery query, CancellationToken cancellationToken)
    {
        var template = await mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                                     && mtp.Id == query.BulkMailTemplateId)
                                          .FirstAsync(cancellationToken);

        var content = query.RegistrationId == null
                          ? template.ContentHtml
                          : await mailComposer.Compose(query.RegistrationId.Value,
                                                       template.ContentHtml ?? string.Empty,
                                                       template.Language,
                                                       cancellationToken);
        return new BulkMailPreview
               {
                   Subject = template.Subject,
                   ContentHtml = content
               };
    }
}
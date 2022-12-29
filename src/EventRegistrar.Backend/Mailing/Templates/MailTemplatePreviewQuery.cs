using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Compose;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTemplatePreviewQuery : IEventBoundRequest, IRequest<MailTemplatePreview>
{
    public Guid EventId { get; set; }
    public Guid MailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class MailTemplatePreview
{
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class AutoMailPreviewQueryHandler : IRequestHandler<MailTemplatePreviewQuery, MailTemplatePreview>
{
    private readonly IQueryable<AutoMailTemplate> _autoMailTemplates;
    private readonly IQueryable<BulkMailTemplate> _bulkMailTemplates;
    private readonly MailComposer _mailComposer;

    public AutoMailPreviewQueryHandler(IQueryable<AutoMailTemplate> autoAutoMailTemplates,
                                       IQueryable<BulkMailTemplate> bulkMailTemplates,
                                       MailComposer mailComposer)
    {
        _autoMailTemplates = autoAutoMailTemplates;
        _bulkMailTemplates = bulkMailTemplates;
        _mailComposer = mailComposer;
    }

    public async Task<MailTemplatePreview> Handle(MailTemplatePreviewQuery query, CancellationToken cancellationToken)
    {
        string? subject;
        string? contentHtml;
        string? language;
        var template = await _autoMailTemplates.Where(mtp => mtp.EventId == query.EventId
                                                          && mtp.Id == query.MailTemplateId)
                                               .FirstOrDefaultAsync(cancellationToken);
        if (template != null)
        {
            subject = template.Subject;
            contentHtml = template.ContentHtml;
            language = template.Language;
        }
        else
        {
            var bulkTemplate = await _bulkMailTemplates.Where(mtp => mtp.EventId == query.EventId
                                                                  && mtp.Id == query.MailTemplateId)
                                                       .FirstAsync(cancellationToken);
            subject = bulkTemplate.Subject;
            contentHtml = bulkTemplate.ContentHtml;
            language = bulkTemplate.Language;
        }


        var content = query.RegistrationId == null
                          ? contentHtml
                          : await _mailComposer.Compose(query.RegistrationId.Value,
                                                        contentHtml ?? string.Empty,
                                                        language,
                                                        cancellationToken);
        return new MailTemplatePreview
               {
                   Subject = subject,
                   ContentHtml = content
               };
    }
}
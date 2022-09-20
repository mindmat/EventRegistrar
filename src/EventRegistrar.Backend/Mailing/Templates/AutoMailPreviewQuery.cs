using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Mailing.Compose;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailPreviewQuery : IEventBoundRequest, IRequest<AutoMailPreview>
{
    public Guid EventId { get; set; }
    public Guid AutoMailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
}

public class AutoMailPreview
{
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class AutoMailPreviewQueryHandler : IRequestHandler<AutoMailPreviewQuery, AutoMailPreview>
{
    private readonly IQueryable<AutoMailTemplate> _mailTemplates;
    private readonly MailComposer _mailComposer;

    public AutoMailPreviewQueryHandler(IQueryable<AutoMailTemplate> mailTemplates,
                                       MailComposer mailComposer)
    {
        _mailTemplates = mailTemplates;
        _mailComposer = mailComposer;
    }

    public async Task<AutoMailPreview> Handle(AutoMailPreviewQuery query, CancellationToken cancellationToken)
    {
        var template = await _mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                                      && mtp.Id == query.AutoMailTemplateId)
                                           .FirstAsync(cancellationToken);

        var content = query.RegistrationId == null
                          ? template.ContentHtml
                          : await _mailComposer.Compose(query.RegistrationId.Value,
                                                        template.ContentHtml ?? string.Empty,
                                                        template.Language,
                                                        cancellationToken);
        return new AutoMailPreview
               {
                   Subject = template.Subject,
                   ContentHtml = content
               };
    }
}
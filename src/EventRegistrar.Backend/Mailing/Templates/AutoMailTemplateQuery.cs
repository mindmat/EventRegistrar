using EventRegistrar.Backend.Authorization;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailTemplateQuery : IEventBoundRequest, IRequest<AutoMailTemplate>
{
    public Guid EventId { get; set; }
    public Guid MailTemplateId { get; set; }
}

public class AutoMailTemplate
{
    public Guid Id { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class AutoMailTemplateQueryHandler : IRequestHandler<AutoMailTemplateQuery, AutoMailTemplate>
{
    private readonly IQueryable<MailTemplate> _mailTemplates;

    public AutoMailTemplateQueryHandler(IQueryable<MailTemplate> mailTemplates)
    {
        _mailTemplates = mailTemplates;
    }

    public async Task<AutoMailTemplate> Handle(AutoMailTemplateQuery query, CancellationToken cancellationToken)
    {
        return await _mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                              && mtp.Id == query.MailTemplateId
                                              && !mtp.IsDeleted)
                                   .Select(mtp => new AutoMailTemplate
                                                  {
                                                      Id = mtp.Id,
                                                      Subject = mtp.Subject,
                                                      ContentHtml = mtp.Template
                                                  })
                                   .FirstAsync(cancellationToken);
    }
}
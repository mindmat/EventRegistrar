﻿namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailTemplateQuery : IEventBoundRequest, IRequest<AutoMailTemplateDisplayItem>
{
    public Guid EventId { get; set; }
    public Guid MailTemplateId { get; set; }
}

public class AutoMailTemplateDisplayItem
{
    public Guid Id { get; set; }
    public MailType Type { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class AutoMailTemplateQueryHandler(IQueryable<AutoMailTemplate> mailTemplates) : IRequestHandler<AutoMailTemplateQuery, AutoMailTemplateDisplayItem>
{
    public async Task<AutoMailTemplateDisplayItem> Handle(AutoMailTemplateQuery query, CancellationToken cancellationToken)
    {
        return await mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                             && mtp.Id == query.MailTemplateId)
                                  .Select(mtp => new AutoMailTemplateDisplayItem
                                                 {
                                                     Id = mtp.Id,
                                                     Type = mtp.Type,
                                                     Subject = mtp.Subject,
                                                     ContentHtml = mtp.ContentHtml
                                                 })
                                  .FirstAsync(cancellationToken);
    }
}
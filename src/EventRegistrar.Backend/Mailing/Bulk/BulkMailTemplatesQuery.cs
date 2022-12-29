namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailTemplatesQuery : IRequest<BulkMailTemplates>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class BulkMailTemplatesQueryHandler : IRequestHandler<BulkMailTemplatesQuery, BulkMailTemplates>
{
    private readonly IQueryable<BulkMailTemplate> _mailTemplates;
    private readonly MailConfiguration _config;

    public BulkMailTemplatesQueryHandler(IQueryable<BulkMailTemplate> mailTemplates,
                                         MailConfiguration config)
    {
        _mailTemplates = mailTemplates;
        _config = config;
    }

    public async Task<BulkMailTemplates> Handle(BulkMailTemplatesQuery query, CancellationToken cancellationToken)
    {
        var existingTemplates = await _mailTemplates.Where(mtp => mtp.EventId == query.EventId)
                                                    .OrderBy(mtp => mtp.BulkMailKey)
                                                    .ThenBy(mtp => mtp.Language)
                                                    .ToListAsync(cancellationToken);
        return new BulkMailTemplates
               {
                   EventId = query.EventId,
                   SenderMail = _config.SenderMail,
                   SenderAlias = _config.SenderName,
                   AvailableLanguages = _config.AvailableLanguages,
                   Keys = existingTemplates.GroupBy(btp => btp.BulkMailKey)
                                           .Select(grp => CreateKey(grp.Key, grp))
               };
    }

    private BulkMailTemplateKey CreateKey(string key, IEnumerable<BulkMailTemplate> existingTemplates)
    {
        var existing = existingTemplates.Where(mtp => mtp.BulkMailKey == key)
                                        .ToList();
        return new BulkMailTemplateKey
               {
                   Key = key,
                   Templates = _config.AvailableLanguages.Select(lng => CreateTemplate(lng, existing.FirstOrDefault(mtp => mtp.Language == lng)))
               };
    }

    private static BulkMailTemplateMetadataLanguage CreateTemplate(string language, BulkMailTemplate? existing)
    {
        return new BulkMailTemplateMetadataLanguage
               {
                   Language = language,
                   Id = existing?.Id,
                   Subject = existing?.Subject
               };
    }
}

public class BulkMailTemplates
{
    public Guid EventId { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderAlias { get; set; }
    public IEnumerable<BulkMailTemplateKey>? Keys { get; set; }
    public IEnumerable<string> AvailableLanguages { get; set; } = null!;
}

public class BulkMailTemplateKey
{
    public string Key { get; set; } = null!;
    public IEnumerable<BulkMailTemplateMetadataLanguage>? Templates { get; set; }
}

public class BulkMailTemplateMetadataLanguage
{
    public Guid? Id { get; set; }
    public string? Language { get; set; }
    public string? Subject { get; set; }
}
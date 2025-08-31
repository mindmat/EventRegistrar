using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Templates.Validation;

namespace EventRegistrar.Backend.Mailing.Templates;

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
    public bool AddIcs { get; set; }
    public string? Warnings { get; set; }
}

public class AutoMailTemplateQueryHandler(IQueryable<AutoMailTemplate> mailTemplates,
                                          IEnumerable<IAutoMailTemplateExpectedPlaceholders> placeholderChecks)
    : IRequestHandler<AutoMailTemplateQuery, AutoMailTemplateDisplayItem>
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
                                      AddIcs = mtp.AddIcs,
                                      ContentHtml = mtp.ContentHtml,
                                      Warnings = GetPlaceholderWarning(placeholderChecks, mtp.FailedPlaceholderChecks)
                                  })
                                  .FirstAsync(cancellationToken);
    }

    private static string? GetPlaceholderWarning(IEnumerable<IAutoMailTemplateExpectedPlaceholders> placeholderChecks,
                                                 ICollection<string>? failedPlaceholderChecks)
    {
        return failedPlaceholderChecks?.Select(fpc => placeholderChecks.FirstOrDefault(pc => pc.GetType().Name == fpc)?.ErrorMessage)
                                       .Where(msg => msg != null)
                                       .Select(msg => msg!)
                                       .StringJoin("\n");
    }
}
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailTemplatesQuery : IRequest<AutoMailTemplates>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class AutoMailTemplatesQueryHandler(IQueryable<AutoMailTemplate> mailTemplates,
                                           MailConfiguration config,
                                           EnumTranslator enumTranslator)
    : IRequestHandler<AutoMailTemplatesQuery, AutoMailTemplates>
{
    public async Task<AutoMailTemplates> Handle(AutoMailTemplatesQuery query, CancellationToken cancellationToken)
    {
        var existingTemplates = await mailTemplates.Where(mtp => mtp.EventId == query.EventId)
                                                   .OrderBy(mtp => mtp.Type)
                                                   .ThenBy(mtp => mtp.Language)
                                                   .ToListAsync(cancellationToken);
        return new AutoMailTemplates
               {
                   EventId = query.EventId,
                   SenderMail = config.SenderMail,
                   SenderAlias = config.SenderName,
                   AvailableLanguages = config.AvailableLanguages,
                   SingleRegistrationPossible = config.SingleRegistrationPossible,
                   PartnerRegistrationPossible = config.PartnerRegistrationPossible,
                   Groups = new[]
                            {
                                new AutoMailTemplateGroup
                                {
                                    Name = Resources.MailTypeGroup_Received,
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .Append(CreateType(MailType.RegistrationReceived, existingTemplates))
                                                      .Append(CreateType(MailType.SoldOut, existingTemplates))
                                                      .AppendIf(config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationAccepted, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPartnerAccepted, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationMatchedAndAccepted, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = Resources.MailTypeGroup_Confirmation,
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationFullyPaid, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPaid, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFullyPaid, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = Resources.MailTypeGroup_WaitingList,
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationOnWaitingList, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPartnerOnWaitingList, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationMatchedOnWaitingList, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = Resources.MailTypeGroup_Reminders,
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationFirstReminder, existingTemplates))
                                                      .AppendIf(config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationSecondReminder, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstReminder, existingTemplates))
                                                      .AppendIf(config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationSecondReminder, existingTemplates))
                                                      .Append(CreateType(MailType.RegistrationCancelled, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = Resources.MailTypeGroup_Payments,
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .Append(CreateType(MailType.MoneyOwed, existingTemplates))
                                                      .Append(CreateType(MailType.TooMuchPaid, existingTemplates))
                                }
                            }
               };
    }

    private AutoMailTemplateMetadataType CreateType(MailType mailType, IEnumerable<AutoMailTemplate> existingTemplates)
    {
        var existing = existingTemplates.Where(mtp => mtp.Type == mailType)
                                        .ToList();
        return new AutoMailTemplateMetadataType
               {
                   Type = mailType,
                   TypeText = enumTranslator.Translate(mailType),
                   ReleaseImmediately = existing.FirstOrDefault()?.ReleaseImmediately,
                   Templates = config.AvailableLanguages.Select(lng => CreateTemplate(lng, existing.FirstOrDefault(mtp => mtp.Language == lng)))
               };
    }

    private static AutoMailTemplateMetadataLanguage CreateTemplate(string language, AutoMailTemplate? existing)
    {
        return new AutoMailTemplateMetadataLanguage
               {
                   Language = language,
                   Id = existing?.Id,
                   Subject = existing?.Subject
               };
    }
}

public class AutoMailTemplates
{
    public Guid EventId { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderAlias { get; set; }
    public IEnumerable<AutoMailTemplateGroup>? Groups { get; set; }
    public IEnumerable<string> AvailableLanguages { get; set; } = null!;
    public bool SingleRegistrationPossible { get; set; }
    public bool PartnerRegistrationPossible { get; set; }
}

public class AutoMailTemplateGroup
{
    public string? Name { get; set; }
    public IEnumerable<AutoMailTemplateMetadataType>? Types { get; set; }
}

public class AutoMailTemplateMetadataType
{
    public MailType Type { get; set; }
    public bool? ReleaseImmediately { get; set; }
    public IEnumerable<AutoMailTemplateMetadataLanguage>? Templates { get; set; }
    public string? TypeText { get; set; }
}

public class AutoMailTemplateMetadataLanguage
{
    public Guid? Id { get; set; }
    public string? Language { get; set; }
    public string? Subject { get; set; }
}
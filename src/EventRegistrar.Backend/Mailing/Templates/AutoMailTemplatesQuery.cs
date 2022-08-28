using EventRegistrar.Backend.Infrastructure;

using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailTemplatesQuery : IRequest<AutoMailTemplates>
{
    public Guid EventId { get; set; }
}

public class AutoMailTemplatesQueryHandler : IRequestHandler<AutoMailTemplatesQuery, AutoMailTemplates>
{
    private readonly IQueryable<MailTemplate> _mailTemplates;
    private readonly MailConfiguration _config;
    private readonly EnumTranslator _enumTranslator;

    public AutoMailTemplatesQueryHandler(IQueryable<MailTemplate> mailTemplates,
                                         MailConfiguration config,
                                         EnumTranslator enumTranslator)
    {
        _mailTemplates = mailTemplates;
        _config = config;
        _enumTranslator = enumTranslator;
    }

    public async Task<AutoMailTemplates> Handle(AutoMailTemplatesQuery query, CancellationToken cancellationToken)
    {
        var existingTemplates = await _mailTemplates.Where(mtp => mtp.EventId == query.EventId
                                                               && mtp.BulkMailKey == null
                                                               && mtp.Type != 0
                                                               && !mtp.IsDeleted)
                                                    .OrderBy(mtp => mtp.Type)
                                                    .ThenBy(mtp => mtp.Language)
                                                    .ToListAsync(cancellationToken);
        return new AutoMailTemplates
               {
                   SenderMail = _config.SenderMail,
                   SenderAlias = _config.SenderName,
                   Groups = new[]
                            {
                                new AutoMailTemplateGroup
                                {
                                    Name = "Received",
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .Append(CreateType(MailType.RegistrationReceived, existingTemplates))
                                                      .Append(CreateType(MailType.SoldOut, existingTemplates))
                                                      .AppendIf(_config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationAccepted, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPartnerAccepted, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationMatchedAndAccepted, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = "Confirmation",
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(_config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationFullyPaid, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPaid, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFullyPaid, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = "Waiting List",
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(_config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationOnWaitingList, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstPartnerOnWaitingList, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationMatchedOnWaitingList, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = "Reminders",
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .AppendIf(_config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationFirstReminder, existingTemplates))
                                                      .AppendIf(_config.SingleRegistrationPossible, () => CreateType(MailType.SingleRegistrationSecondReminder, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationFirstReminder, existingTemplates))
                                                      .AppendIf(_config.PartnerRegistrationPossible, () => CreateType(MailType.PartnerRegistrationSecondReminder, existingTemplates))
                                },
                                new AutoMailTemplateGroup
                                {
                                    Name = "Payments",
                                    Types = Enumerable.Empty<AutoMailTemplateMetadataType>()
                                                      .Append(CreateType(MailType.MoneyOwed, existingTemplates))
                                                      .Append(CreateType(MailType.TooMuchPaid, existingTemplates))
                                }
                            }
               };
    }

    private AutoMailTemplateMetadataType CreateType(MailType mailType, IEnumerable<MailTemplate> existingTemplates)
    {
        var existing = existingTemplates.Where(mtp => mtp.Type == mailType)
                                        .ToList();
        return new AutoMailTemplateMetadataType
               {
                   Type = mailType,
                   TypeText = _enumTranslator.Translate(mailType),
                   ReleaseImmediately = existing.FirstOrDefault()?.ReleaseImmediately ?? false,
                   Templates = _config.AvailableLanguages.Select(lng => CreateTemplate(lng, existing.FirstOrDefault(mtp => mtp.Language == lng)))
               };
    }

    private AutoMailTemplateMetadataLanguage CreateTemplate(string language, MailTemplate? existing)
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
    public string? SenderMail { get; set; }
    public string? SenderAlias { get; set; }
    public IEnumerable<AutoMailTemplateGroup>? Groups { get; set; }
}

public class AutoMailTemplateGroup
{
    public string? Name { get; set; }
    public IEnumerable<AutoMailTemplateMetadataType>? Types { get; set; }
}

public class AutoMailTemplateMetadataType
{
    public MailType Type { get; set; }
    public bool ReleaseImmediately { get; set; }
    public IEnumerable<AutoMailTemplateMetadataLanguage>? Templates { get; set; }
    public string? TypeText { get; set; }
}

public class AutoMailTemplateMetadataLanguage
{
    public Guid? Id { get; set; }
    public string? Language { get; set; }
    public string? Subject { get; set; }
}
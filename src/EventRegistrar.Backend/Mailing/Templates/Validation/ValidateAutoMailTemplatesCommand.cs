using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Templates.Validation;

public class ValidateAutoMailTemplatesCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid? AutoMailTemplateId { get; set; }
}

public class ValidateAutoMailTemplatesCommandHandler(IQueryable<AutoMailTemplate> templates,
                                                     IEnumerable<IAutoMailTemplateExpectedPlaceholders> expectedPlaceholders,
                                                     ChangeTrigger changeTrigger)
    : IRequestHandler<ValidateAutoMailTemplatesCommand>
{
    //private static Dictionary<MailType, string[][]> expectedPlaceholders = new()
    //{
    //    { MailType.PartnerRegistrationMatchedAndAccepted, [
    //        [$"{Role.Leader}.{MailPlaceholder.FirstName}", $"{Role.Leader}.{MailPlaceholder.LastName}"],
    //        [$"{Role.Follower}.{MailPlaceholder.FirstName}", $"{Role.Follower}.{MailPlaceholder.LastName}"],
    //        [$"{Role.Leader}.{MailPlaceholder.DueAmount}", $"{Role.Leader}.{MailPlaceholder.QrCode}"],
    //        [$"{Role.Follower}.{MailPlaceholder.DueAmount}", $"{Role.Follower}.{MailPlaceholder.QrCode}"],
    //    ] },
    //};
    public async Task Handle(ValidateAutoMailTemplatesCommand command, CancellationToken cancellationToken)
    {
        var mailTypesToCheck = expectedPlaceholders.SelectMany(eph => eph.AppliesTo);
        var templatesToCheck = await templates.Where(amt => amt.EventId == command.EventId
                                                         && mailTypesToCheck.Contains(amt.Type))
                                              .WhereIf(command.AutoMailTemplateId != null,
                                                       amt => amt.Id == command.AutoMailTemplateId)
                                              .ToListAsync(cancellationToken);
        foreach (var template in templatesToCheck)
        {
            var content = template.ContentHtml ?? string.Empty;
            template.FailedPlaceholderChecks = expectedPlaceholders.Where(eph => eph.AppliesTo.Contains(template.Type))
                                                   .Where(eph => eph.MissesPlaceholder(content))
                                                   .Select(eph => eph.GetType().Name)
                                                   .ToList();
        }

        //changeTrigger.TriggerUpdate<>();
    }
}

public interface IAutoMailTemplateExpectedPlaceholders
{
    public MailType[] AppliesTo { get; }
    public bool MissesPlaceholder(string content);
    public string ErrorMessage { get; }
}

public class BothNamesExpectedInPartnerMail : IAutoMailTemplateExpectedPlaceholders
{
    public MailType[] AppliesTo { get; } =
    [
        MailType.PartnerRegistrationMatchedAndAccepted,
        MailType.PartnerRegistrationFullyPaid,
        MailType.PartnerRegistrationFirstReminder,
        MailType.PartnerRegistrationSecondReminder,
        MailType.PartnerRegistrationMatchedOnWaitingList
    ];

    public bool MissesPlaceholder(string content)
    {
        return !content.Contains($"{Role.Leader}.{MailPlaceholder.FirstName}") && !content.Contains($"{Role.Leader}.{MailPlaceholder.LastName}")
            || !content.Contains($"{Role.Follower}.{MailPlaceholder.FirstName}") && !content.Contains($"{Role.Follower}.{MailPlaceholder.LastName}");
    }

    public string ErrorMessage { get; } = string.Format(Resources.BothNamesExpectedInPartnerMail,
                                                        $"{Role.Leader}.{MailPlaceholder.FirstName}",
                                                        $"{Role.Follower}.{MailPlaceholder.FirstName}");
}